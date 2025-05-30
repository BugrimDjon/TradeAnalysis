// Services/OkxTradeAnalysisService.cs
using bot_analysis.Enums;
using bot_analysis.Interfaces;
using bot_analysis.Models;
using bot_analysis.Models.OKX;
using System.Data;
using System.Globalization;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

//using bot_analysis.Services;

namespace bot_analysis.Services.OKX
{



    /// <summary>
    /// Анализ сделок по биржи ОКХ
    /// </summary>
    public class OkxTradeAnalysisService : ITradeAnalysisService
    {
        private readonly ITradeApiClient _apiClient;
        private readonly IWorkWithDataBase _dataBase;
        private readonly ILogger _logger;
        private readonly JsonSerializerOptions _jsonOptions;


        //конструктор класса
        public OkxTradeAnalysisService(ITradeApiClient apiClient, IWorkWithDataBase dataBase, JsonSerializerOptions jsonjsonOptions, ILogger logger)
        {
            _apiClient = apiClient;
            _dataBase = dataBase;
            _logger = logger;
            _jsonOptions = jsonjsonOptions;
        }

        //обновить затраченные средства для переводов на/c аккаунта,
        //для последующего учета средней цены покупки монеты. Переведенные средства будут учитываться
        //как покупка по цене на момент перевода
        //
        public async Task UpdateTransferEvaluationsAsync()
        {
            //в bills_table поле type=1 - это соответствует переводам на/с аккаунта
            //найти в таблице bills_table такие поля billId, которые отсутствуют
            //в таблице transfer_valuations и для этих записей скопировать поля
            //billId, sz из таблицы bills_table в transfer_valuations
            string query = @"INSERT INTO transfer_valuations (billId, sz)
                            SELECT b.billId, b.sz
                            FROM bills_table b
                            LEFT JOIN transfer_valuations t ON b.billId = t.billId
                            WHERE b.type = 1 AND t.billId IS NULL;";
            await _dataBase.ExecuteSQLQueryWithoutReturningParameters(query);

            //получить поля billId, fillTime, ccy из таблицы bills_table
            //у которых совпадают billId и поле px в таблице transfer_valuations равно NULL
            query = @"SELECT b.billId ,b.fillTime, b.ccy
                            FROM transfer_valuations t
                            JOIN bills_table b ON b.billId = t.billId
                            WHERE t.px IS NULL
                            ORDER BY b.fillTime";
            DataTable dataTable = await _dataBase.ExecuteSqlQueryReturnDataTable(query);
            //dataTable.Columns.Add("px");

            string px;
            foreach (DataRow tempDataTable in dataTable.Rows)
            {
                string? coin = tempDataTable["ccy"].ToString();
                if (coin == "USDT")
                {
                    px = "1";
                }
                else
                {
                    //округляем время перевода до 1 минуты
                    long.TryParse(tempDataTable["fillTime"].ToString(), out long fillTime);
                    string? point = (((fillTime + 59999) / 60000) * 60000).ToString();
                    //производим запрос свечи на эту минуту
                    var candleData = await _apiClient.GetApiDataAsDataTableUniversalAsync(
                                                OkxEndPoints.Candles(coin + "-USDT", "1m", "1"),
                                                PaginationDirection.After,
                                                point);
                    //если ответ не содержит строк - ответ пустой
                    if (candleData.Rows.Count == 0)
                    {
                        _logger.Error($"Нет свечей для {coin} на {point}");
                    }
                    decimal.TryParse(candleData.Rows[0][3].ToString(), NumberStyles.Any,
                        CultureInfo.InvariantCulture, out decimal highPrice);
                    decimal.TryParse(candleData.Rows[0][4].ToString(), NumberStyles.Any,
                        CultureInfo.InvariantCulture, out decimal lowPrice);
                    decimal resultPrase = (highPrice + lowPrice) / 2;
                    px = resultPrase.ToString(CultureInfo.InvariantCulture);
                }
                // Сразу обновляем базу
                string updateQuery = $@"UPDATE transfer_valuations 
                            SET px = '{px}' 
                            WHERE billId = '{tempDataTable["billId"]}'";
                await _dataBase.ExecuteSQLQueryWithoutReturningParameters(updateQuery);
            }
        }




        public async Task UpdateBotsAsync()
        {
            _logger.Info("Начало");

            //вычитка ботов звкончивших работу
            await UpdateStoppedRunningBotsBypassingBifoBugAsync(true);
            //вычитка работающих ботов
            await UpdateStoppedRunningBotsBypassingBifoBugAsync(false);
            //обновление сделок по остановленным ботам
            await UpdateOrdBotsВypassingBifoBug(true);
            //обновление сделок по работающим ботам
            await UpdateOrdBotsВypassingBifoBug(false);
        }

        public async Task UpdateBalansAcauntAsync()
        {
            var balans = await _apiClient.GetApiDataAsync<ApiOkxBalans, OkxBalans>(OkxEndPoints.BalansAcaunt);
        }



        private async Task UpdateOrdBotsВypassingBifoBug(bool oldBot)
        {
            int len;
            int counter;
            string pointRead = "";
            string query;
            bool goalHasBeenAchieved;
            string targetPointRead = "";
            int allLen = 0;
            IEnumerable<OkxBotOrder> pageData;

            query = oldBot

                 //подготовить запрос для перечня ботов, которые остановленны и
                 //не обработаны (переписать все сделки) или не верефицированы
                 //верефицированы - повторная обработка на раньше чем через 20 минут после остановки
                 ? @"select AlgoId from gridbots
                            where state='stopped' and (IsProcessed=0 or IsVerified=0)
                            order by ctime 	asc;"

                 //подготовить запрос для работающих ботов ботов
                 : @"select AlgoId from gridbots
                            where state='running'
                            order by ctime 	asc;";


            //получить перечень ботов согластно запроса
            var data = await _dataBase.ExecuteSqlQueryReturnParamListString(query);

            foreach (var algoId in data)
            {
                counter = 0;
                len = 0;
                goalHasBeenAchieved = false;
                targetPointRead = "";
                _logger.Info("Производим обработку данных бота AlgoId = " + algoId);

                if (!oldBot)
                {   //Если это работающий бот
                    //найти ID сделки с которой надо производить вычитку.
                    //эта ID сделка имеет время обновления на 20 позиций раньше чем самая новая
                    //для исключения некоректных данных при последнем считывании
                    //то есть перезапишет данные по последним 20 сделкам и будет писать новые

                    query = $@"select ordId from bot_orders
                        where algoId = {algoId}
                        ORDER BY cTime DESC
                        LIMIT 1 offset 20;";
                    targetPointRead = await _dataBase.ExecuteSqlQueryReturnParamString(query);
                }

                do
                {
                    ////ограничение скорости вызова запроса 10 запросов в 1 секунды
                    //await RateLimiter.EnforceRateLimit(10);
                    if (counter == 0)
                    {
                        //если выполнились условия то это первый запрос

                        pageData = await _apiClient.GetApiDataAsync<ApiOkxBotOrder, OkxBotOrder>(
                                    OkxEndPoints.SubOrdersBot(algoId));
                    }
                    else
                    {
                        //считываем данные от новых к старым
                        pageData = await _apiClient.GetApiDataAsync<ApiOkxBotOrder, OkxBotOrder>(
                                OkxEndPoints.SubOrdersBot(algoId),
                                PaginationDirection.After, pointRead);
                    }

                    //сохраняем полученные данные в БД
                    await _dataBase.SaveOrdStoppedBotsToDB(pageData);

                    //ищим совпадение целевой сделки в полученных данных
                    foreach (var entryPageData in pageData)
                    {
                        len++;
                        if (entryPageData.ordId == targetPointRead)
                        {
                            goalHasBeenAchieved = true;
                            break;
                        }
                    }

                    if (pageData.Count() < 100 || goalHasBeenAchieved)
                    {
                        _logger.Info("Закончили обработку данных бота AlgoId = " + algoId +
                            "      Считано " + (len-20) + " записей");

                        break;
                    }
                    pointRead = pageData.LastOrDefault()?.ordId ?? "";
                    counter++;
                }
                while (true);

                //найти время окончания работы бота с ID = algoId
                // если он stopped
                query = $@"select UTime from gridbots
                            where AlgoId={algoId} and State='stopped';";

                string uTime = await _dataBase.ExecuteSqlQueryReturnParamString(query);

                if (!string.IsNullOrEmpty(uTime))
                {
                    //если время завершения работы бота более 20 мин,
                    //то считаем что все данные по работе этого бота рассчитаны
                    if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() -
                               long.Parse(uTime) > 20 * 60 * 1000)
                    {
                        //установить статус бота в БД "верефицирован"
                        query = "UPDATE gridbots SET IsVerified = true WHERE AlgoId =" + algoId;
                        await _dataBase.ExecuteSQLQueryWithoutReturningParameters(query);
                    }

                    //установить статус бота в БД "обработан"
                    query = "UPDATE gridbots SET IsProcessed = true WHERE AlgoId =" + algoId;
                    await _dataBase.ExecuteSQLQueryWithoutReturningParameters(query);
                }

                allLen += len;
            }
            _logger.Info("Общее кол во записей - " + (allLen-20));
        }
        private async Task UpdateStoppedRunningBotsBypassingBifoBugAsync(bool stoppedBot)
        {
            int len = 0;
            int counter = 0;
            string pointRead = "";
            string targetPointRead = "";
            bool goalHasBeenAchieved = false;

            IEnumerable<OkxBot> pageData;

            //нахотим точку отсчета с которой производить считывание
            if (stoppedBot)
            {
                /// находит в таблице `gridbots` AlgoId такого бота, который не работает и имеет время
                /// создания меньше или равное времени создания самого раннего работающего бота
                const string query = @"
                                   SELECT AlgoId
                                    FROM gridbots
                                    WHERE CTime <= (
                                        SELECT MIN(CTime)
                                        FROM gridbots
                                        WHERE State = 'running'
                                    ) and State = 'stopped'
                                    ORDER BY CTime DESC
                                    LIMIT 1;";
                targetPointRead = await _dataBase.ExecuteSqlQueryReturnParamString(query);
                //pointRead = await _dataBase.SearchPointToReadNewDataForStoppedBot();
            }
            else
            {
                pointRead = "";
            }

            do
            {
                /*//ограничение скорости вызова запроса 10 запросов в 1 секунды
                await RateLimiter.EnforceRateLimit(10);*/

                var urlBot = stoppedBot ? OkxEndPoints.StoppedBot : OkxEndPoints.RunningBot;
                //если counter==0 то это первое считывание и читаем без пагинации
                pageData = counter == 0
                    ? await _apiClient.GetApiDataAsync<ApiOkxBot, OkxBot>(urlBot)
                    //читаем с пагинацией
                    : await _apiClient.GetApiDataAsync<ApiOkxBot, OkxBot>(urlBot, PaginationDirection.After, pointRead);

                //сохраняем полученные данные в БД
                await _dataBase.SavePageBotToDataBase(pageData);

                //достигнут конечный результат
                foreach (var entryPageData in pageData)
                {
                    len++;
                    if (entryPageData.AlgoId == targetPointRead)
                    {
                        goalHasBeenAchieved = true;
                        break;
                    }
                }

                if (pageData.Count() < 100 || goalHasBeenAchieved)
                {
                    string temp = stoppedBot
                        ? "Закончили запрос по ботам закончивших работу. "
                        : "Закончили запрос по работающим ботам. ";

                    _logger.Info(temp + "      Обработано " + len + " записей");
                    break;
                }

                pointRead = pageData.LastOrDefault()?.AlgoId ?? "";
                counter++;
            }
            while (true);
        }

        //генерация отчета
        public async Task<IEnumerable<OkxReport>> GenerateReport()
        {
            string query;
            var report = new List<OkxReport>();
            var reportV2 = new List<OkxReportV_2>();
            int counter = 0;
            var UniqueCoins = await _dataBase.GetUniqueCoinsAsync();

            //считать текуший баланс аккаунта
            IEnumerable<OkxBalans> balans = await _apiClient.GetApiDataAsync<ApiOkxBalans, OkxBalans>(OkxEndPoints.BalansAcaunt);


            foreach (var coin in UniqueCoins)
            {
                var tempReport = new OkxReport();
                var tempReportV2 = new OkxReportV_2();

                //Перечень монет
                tempReport.Coins = coin;
                tempReportV2.Coins = tempReport.Coins;

                //Зачислено монет
                query = $@" SELECT SUM(balChg)
                      FROM `bills_table`
                      where `type`= '1' and ccy = '{coin}' and balChg>0;";
                tempReport.Deposit = await _dataBase.ExecuteSqlQueryReturnParamString(query);
                tempReportV2.Deposit = tempReport.Deposit;

                //На какую сумму оценочно
                query = $@" SELECT SUM(t.expense_amount_usdt)
                        FROM transfer_valuations t
                        join bills_table b on b.billId = t.billId
                        where b.ccy = '{coin}' and t.expense_amount_usdt>0";
                tempReport.DepositSum = await _dataBase.ExecuteSqlQueryReturnParamString(query);
                tempReportV2.DepositSum = tempReport.DepositSum;
                
                //выведено монет
                query = $@" SELECT SUM(balChg)
                      FROM `bills_table`
                      where `type`= '1' and ccy = '{coin}' and balChg<0;";
                tempReport.Withdraw = await _dataBase.ExecuteSqlQueryReturnParamString(query);
                tempReportV2.Withdraw = tempReport.Withdraw;

                //На какую сумму оценочно
                query = $@" SELECT SUM(t.expense_amount_usdt)
                        FROM transfer_valuations t
                        join bills_table b on b.billId = t.billId
                        where b.ccy = '{coin}' and t.expense_amount_usdt<0";
                tempReport.WithdrawSum = await _dataBase.ExecuteSqlQueryReturnParamString(query);
                tempReportV2.WithdrawSum = tempReport.WithdrawSum;

                decimal.TryParse(tempReport.Deposit?.Replace(',', '.'), NumberStyles.Any,
                        CultureInfo.InvariantCulture, out decimal a);
                decimal.TryParse(tempReport.Withdraw?.Replace(',', '.'), NumberStyles.Any,
                        CultureInfo.InvariantCulture, out decimal b);
                tempReport.DepositWithdraw = (a + b).ToString();

                decimal.TryParse(tempReport.DepositSum?.Replace(',', '.'), NumberStyles.Any,
                        CultureInfo.InvariantCulture, out a);
                decimal.TryParse(tempReport.WithdrawSum?.Replace(',', '.'), NumberStyles.Any,
                        CultureInfo.InvariantCulture, out b);
                tempReport.DepositWithdrawSum = (a + b).ToString();

                var tempRow = balans.FirstOrDefault();

                foreach (var temp in tempRow.Details)
                {
                    if (temp.Ccy == coin)
                    {
                        tempReport.Eq = temp.Eq?.Replace(".", ",");
                        tempReportV2.Eq = temp.Eq;

                        tempReport.Equsd = temp.Equsd?.Replace(".", ",");
                        tempReportV2.Equsd = tempReport.Equsd;

                        tempReport.Availbal = temp.Availbal?.Replace(".", ",");
                        tempReportV2.Availbal = tempReport.Availbal;

                        tempReport.Cashbal = temp.Cashbal?.Replace(".", ",");
                        tempReportV2.AvailbalDetails.Cashbal= tempReport.Cashbal ?? "";

                        tempReport.Ordfrozen = temp.Ordfrozen?.Replace(".", ",");
                        tempReportV2.AvailbalDetails.Ordfrozen= tempReport.Ordfrozen ?? "";
                        
                        tempReport.Stgyeq = temp.Stgyeq?.Replace(".", ",");
                        tempReportV2.AvailbalDetails.Stgyeq= tempReport.Stgyeq  ?? "" ;
                        
                        tempReport.Frozenbal = temp.Frozenbal?.Replace(".", ",");
                        tempReportV2.Frozenbal = tempReport.Frozenbal;
                        
                        tempReport.Spotbal = temp.Spotbal?.Replace(".", ",");
                        tempReportV2.Spotbal = tempReport.Spotbal;

                    }
                }

                //Количество купленных монет
                query = $@" SELECT SUM(balChg)
                      FROM `bills_table`
                      where `type`= '2' and ccy = '{coin}'  and subType ='1';";
                tempReport.BuyAmount = await _dataBase.ExecuteSqlQueryReturnParamString(query);
                tempReportV2.BuyAmount = tempReport.BuyAmount;

                //На сумму в USDT
                query = $@" SELECT abs(SUM(balChg))
                        FROM `bills_table`
                        where `type`= ""2"" and ccy = 'usdt' and
                               instId = '{coin}'""-USDT"" and subType='2';";
                tempReport.BuyTotal = await _dataBase.ExecuteSqlQueryReturnParamString(query);
                tempReportV2.BuyTotal = tempReport.BuyTotal;

                if (!string.IsNullOrWhiteSpace(tempReport.BuyAmount) &&
                    !string.IsNullOrWhiteSpace(tempReport.BuyTotal))
                {
                    //Средняя цена покупки
                    tempReport.BuyAvgPrice = Convert.ToString(
                        Convert.ToDecimal(tempReport.BuyTotal) /
                        Convert.ToDecimal(tempReport.BuyAmount));

                    if (!string.IsNullOrWhiteSpace(tempReport.DepositSum) &&
                    !string.IsNullOrWhiteSpace(tempReport.Deposit))
                    {
                        //Средняя цена покупки с учетом переводов
                        tempReport.BuyAvgPriceIncludingTransfers = Convert.ToString(
                        (Convert.ToDecimal(tempReport.BuyTotal) + Convert.ToDecimal(tempReport.DepositSum)) /
                        (Convert.ToDecimal(tempReport.BuyAmount) + Convert.ToDecimal(tempReport.Deposit)));
                    }
                    else
                    {
                        tempReport.BuyAvgPriceIncludingTransfers = tempReport.BuyAvgPrice;
                        tempReportV2.BuyAvgPriceIncludingTransfers = tempReport.BuyAvgPriceIncludingTransfers;
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(tempReport.DepositSum) &&
                    !string.IsNullOrWhiteSpace(tempReport.Deposit))
                    {
                        //Средняя цена покупки с учетом переводов
                        tempReport.BuyAvgPriceIncludingTransfers = Convert.ToString(
                         Convert.ToDecimal(tempReport.DepositSum) /
                         Convert.ToDecimal(tempReport.Deposit));
                    }
                    else
                    {
                        tempReport.BuyAvgPriceIncludingTransfers = tempReport.BuyAvgPrice;
                        tempReportV2.BuyAvgPriceIncludingTransfers = tempReport.BuyAvgPriceIncludingTransfers;
                    }
                }

                //Количество проданных монет
                query = $@" SELECT abs(SUM(balChg))
                      FROM `bills_table`
                      where `type`= '2' and ccy = '{coin}'  and subType ='2';";
                tempReport.SellAmount = await _dataBase.ExecuteSqlQueryReturnParamString(query);
                tempReportV2.SellAmount = tempReport.SellAmount;

                //На сумму в USDT
                query = $@" SELECT abs(SUM(balChg))
                        FROM `bills_table`
                        where `type`= ""2"" and ccy = 'usdt' and
                               instId = '{coin}'""-USDT"" and subType='1';";
                tempReport.SellTotal = await _dataBase.ExecuteSqlQueryReturnParamString(query);
                tempReportV2.SellTotal = tempReport.SellTotal;

                if (!string.IsNullOrWhiteSpace(tempReport.SellAmount) &&
                    !string.IsNullOrWhiteSpace(tempReport.SellTotal))
                {
                    //Средняя цена продажи
                    tempReport.SellAvgPrice = Convert.ToString(
                        Convert.ToDecimal(tempReport.SellTotal) /
                        Convert.ToDecimal(tempReport.SellAmount));

                    if (!string.IsNullOrWhiteSpace(tempReport.WithdrawSum) &&
                        !string.IsNullOrWhiteSpace(tempReport.Withdraw))

                    {
                        //Средняя цена продажи с учетом переводов
                        tempReport.SelAvgPriceIncludingTransfers = Convert.ToString(
                       (Convert.ToDecimal(tempReport.SellTotal) +
                       Convert.ToDecimal(tempReport.WithdrawSum)) /
                        (Convert.ToDecimal(tempReport.SellAmount) + Convert.ToDecimal(tempReport.Withdraw)));
                    }
                    else
                    {
                        tempReport.SelAvgPriceIncludingTransfers = tempReport.SellAvgPrice;
                        tempReportV2.SelAvgPriceIncludingTransfers = tempReport.SelAvgPriceIncludingTransfers;
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(tempReport.WithdrawSum) &&
                        !string.IsNullOrWhiteSpace(tempReport.Withdraw))

                    {
                        //Средняя цена продажи с учетом переводов
                        tempReport.SelAvgPriceIncludingTransfers = Convert.ToString(
                            Convert.ToDecimal(tempReport.WithdrawSum) /
                            Convert.ToDecimal(tempReport.Withdraw));
                    }
                    else
                    {
                        tempReport.SelAvgPriceIncludingTransfers = tempReport.SellAvgPrice;
                        tempReportV2.SelAvgPriceIncludingTransfers = tempReport.SelAvgPriceIncludingTransfers;
                    }
                }





                //****************Следующие параметры не проверены

                // Количество купленных монет ботом
                query = $@" select SUM(coin_delta)
                      from bot_orders
                      where instId = '{coin}-USDT' and coin_delta>0;";

                tempReport.BuyAmountBot = await _dataBase.ExecuteSqlQueryReturnParamString(query);
                tempReportV2.BuyAmountBot = tempReport.BuyAmountBot;


                // купленных ботом на сумму в USDT
                query = $@" select abs(SUM(usdt_delta))
                      from bot_orders
                      where instId = '{coin}-USDT' and usdt_delta<0;";

                tempReport.BuyTotalBot = await _dataBase.ExecuteSqlQueryReturnParamString(query);
                tempReportV2.BuyTotalBot = tempReport.BuyTotalBot;

                //% дохода
                if (!string.IsNullOrWhiteSpace(tempReport.BuyAvgPriceIncludingTransfers) &&
                   !string.IsNullOrWhiteSpace((string?)tempReport.SelAvgPriceIncludingTransfers))
                {
                    var buy = Convert.ToDecimal(tempReport.BuyAvgPriceIncludingTransfers);
                    var sell= Convert.ToDecimal(tempReport.SelAvgPriceIncludingTransfers);
                    tempReport.ProfitPercent = Convert.ToString(
                        (sell - buy) / buy* 100);
                    tempReportV2.ProfitPercent = tempReport.ProfitPercent;
                }





                //Монет в наличии 
                query = $@" SELECT SUM(balChg)
                        FROM `bills_table`
                        where ccy = '{coin}';";
                //Console.WriteLine(query);
                tempReport.CurrentAmount = await _dataBase.ExecuteSqlQueryReturnParamString(query);
                tempReportV2.CurrentAmount = tempReport.CurrentAmount;

                report.Add(tempReport);
                reportV2.Add(tempReportV2);
                counter++;
            }


/*           
            var trans = ObjectTransposer.TransposeObjects(reportV2.Cast<object>());
    
            await using FileStream stream2 =
            
                File.Create("C:\\Users\\Djon\\source\\repos\\bot_analysis\\для теста\\TReportV2.json");
            await JsonSerializer.SerializeAsync(stream2, trans, _jsonOptions);
*/

            return report;
        }

        //Сохранить отчет
        public async Task GenerateReportAsync<T>(IEnumerable<T> data) where T : class 
        {
            await using FileStream stream =
                File.Create("C:\\Users\\Djon\\source\\repos\\bot_analysis\\для теста\\Report.json");
            await JsonSerializer.SerializeAsync(stream, data, _jsonOptions);
            var tData = ObjectTransposer.TransposeObjects(data.Cast<object>());
            await using FileStream stream2 = File.Create("C:\\Users\\Djon\\source\\repos\\bot_analysis_veb\\report\\public\\Report.json");
            await JsonSerializer.SerializeAsync(stream2, tData, _jsonOptions);
        }

        public async Task UpdateUniqueTradingPairsAsync()
        {
            await _dataBase.UpdateUniqueTradingPairsInBD();
        }

        public async Task UpdateUniqueCoinsAsync()
        {
            await _dataBase.UpdateUniqueCoinsInBD();
        }

        /// <summary>
        /// Метод для обновления переводов на/с торгового счета
        /// </summary>
        /// <returns> нет возвращаемых параметров </returns>
        public async Task UpdateAccountTransfersAsync()
        {
            await UpdateAccountTransfersВypassingBifoBugAsync();
        }

        /// <summary>
        /// Метод для переводов с/на аккаунт
        /// </summary>
        /// <returns> нет возвращаемых параметров </returns>
        private async Task UpdateAccountTransfersВypassingBifoBugAsync()
        {
            int len = 0;
            int counter = 0;
            string pointRead;
            bool startedOver = false;

            IEnumerable<OkxBill> pageData;

            //нахотим точку отсчета с которой производить считывание
            /// находит в таблице `gridbots` billid такой сделки, которая состоялась
            /// на 20 позиций раньше чем  самая новая
            const string query = @"
                                    SELECT billid
                                    FROM bills_table
                                    ORDER BY fillTime DESC
                                    LIMIT 1 OFFSET 21;";
            pointRead = await _dataBase.ExecuteSqlQueryReturnParamString(query);
            do
            {
                /*//ограничение скорости вызова запроса 5 запросов в 1 секунды
                await RateLimiter.EnforceRateLimit(5);*/

                if (string.IsNullOrEmpty(pointRead) && counter == 0)
                {
                    //если выполнились условия то это первый запрос
                    pageData = await _apiClient.GetApiDataAsync<ApiOkxBill, OkxBill>
                                (OkxEndPoints.Bill);
                    startedOver = true;
                }
                else
                {
                    if (startedOver)
                    {
                        //считываем данные от новых к старым
                        pageData = await _apiClient.GetApiDataAsync<ApiOkxBill, OkxBill>
                                (OkxEndPoints.Bill,
                                PaginationDirection.After, pointRead);
                    }
                    else
                    {
                        //считываем данные от станых к новым
                        pageData = await _apiClient.GetApiDataAsync<ApiOkxBill, OkxBill>
                                (OkxEndPoints.Bill,
                                PaginationDirection.Before, pointRead);
                    }
                }

                //сохраняем полученные данные в БД
                await _dataBase.SavePageAccountTransfersToDataBase(pageData);
                //достигнут конечный результат

                if (pageData.Count() < 100)
                {
                    len += pageData.Count();
                    _logger.Info("Закончили запрос по транзакциям ботов. " +
                        "      Обработано " + (len-21) + " записей");
                    break;
                }

                len += pageData.Count();
                if (startedOver)
                    pointRead = pageData.LastOrDefault()?.BillId ?? "";
                else
                    pointRead = pageData.FirstOrDefault()?.BillId ?? "";
                _logger.Info("Обработано " + (len-21) + " записей");
                counter++;
            }
            while (true);
        }

        /// <summary>
        /// Метод для обновления ручных сделок
        /// </summary>
        /// <returns> нет возвращаемых параметров </returns>
        public async Task UpdateTradesAsync()
        {
            await UpdateTradesВypassingBifoBugAsync();
        }

        /// <summary>
        /// Метод для переводов с/на аккаунт
        /// </summary>
        /// <returns> нет возвращаемых параметров </returns>
        private async Task UpdateTradesВypassingBifoBugAsync()
        {
            int len = 0;
            int counter = 0;
            string pointRead;
            bool startedOver = false;
            IEnumerable<OkxTradeFillsHistory> pageData;

            //нахотим точку отсчета с которой производить считывание
            /// находит в таблице `gridbots` billid такой сделки, которая состоялась
            /// на 20 позиций раньше чем  самая новая
            const string query = @"
                                    SELECT billid
                                    FROM tradefills
                                    ORDER BY fillTime DESC
                                    LIMIT 1 OFFSET 20;";
            pointRead = await _dataBase.ExecuteSqlQueryReturnParamString(query);
            do
            {
                /* //ограничение скорости вызова запроса 5 запросов в 1 секунды
                 await RateLimiter.EnforceRateLimit(5);*/

                if (string.IsNullOrEmpty(pointRead) && counter == 0)
                {
                    //если выполнились условия то это первый запрос
                    pageData = await _apiClient.GetApiDataAsync
                        <ApiOkxTradeFillsHistory, OkxTradeFillsHistory>
                        (OkxEndPoints.FillHistorySpot);
                    startedOver = true;
                }
                else
                {
                    if (startedOver)
                    {
                        //считываем данные от новых к старым
                        pageData = await _apiClient.GetApiDataAsync<ApiOkxTradeFillsHistory, OkxTradeFillsHistory>
                                (OkxEndPoints.FillHistorySpot,
                                PaginationDirection.After, pointRead);
                    }
                    else
                    {
                        //считываем данные от старых к новым
                        pageData = await _apiClient.GetApiDataAsync<ApiOkxTradeFillsHistory, OkxTradeFillsHistory>
                                (OkxEndPoints.FillHistorySpot,
                                PaginationDirection.Before, pointRead);
                    }
                }

                //сохраняем полученные данные в БД
                await _dataBase.SavePageTradeFillsHistoryToDataBase(pageData);
                //достигнут конечный результат

                if (pageData.Count() < 100)
                {
                    len += pageData.Count();
                    _logger.Info("Закончили запрос по всем транзакциям аккаунта. " +
                        "      Обработано " + (len-20) + " записей");
                    break;
                }

                len += pageData.Count();
                if (startedOver)
                    pointRead = pageData.LastOrDefault()?.billId ?? "";
                else
                    pointRead = pageData.FirstOrDefault()?.billId ?? "";
                _logger.Info("Обработано " + (len-20) + " записей");
                counter++;
            }
            while (true);
        }
    }
}
