// Services/OkxTradeAnalysisService.cs
using bot_analysis.Enums;
using bot_analysis.Interfaces;
using bot_analysis.Models.OKX;
using System.Text.Json;
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
                    //ограничение скорости вызова запроса 10 запросов в 1 секунды
                    await RateLimiter.EnforceRateLimit(10);
                    if (counter == 0)
                    {
                        //если выполнились условия то это первый запрос

                        pageData = await _apiClient.GetApiDataAsync<ApiOkxBotOrder, OkxBotOrder>(
                                    OkxUrlConst.SubOrdersBot(algoId));
                    }
                    else
                    {
                        //считываем данные от новых к старым
                        pageData = await _apiClient.GetApiDataAsync<ApiOkxBotOrder, OkxBotOrder>(
                                OkxUrlConst.SubOrdersBot(algoId),
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
                            "      Считано " + len + " записей");

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

            Console.WriteLine("Общее кол во записей - " + allLen);
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
                //ограничение скорости вызова запроса 10 запросов в 1 секунды
                await RateLimiter.EnforceRateLimit(10);

                string urlBot = stoppedBot ? OkxUrlConst.StoppedBot : OkxUrlConst.RunningBot;
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
            int counter = 0;
            var UniqueCoins = await _dataBase.GetUniqueCoinsAsync();

            foreach (var coin in UniqueCoins)
            {
                var tempReport = new OkxReport();

                //Перечень монет
                tempReport.Coins = coin;

                //Количество переведенных монет
                query = $@" SELECT SUM(balChg)
                      FROM `bills_table`
                      where `type`= '1' and ccy = '{coin}';";

                tempReport.CoinsTransf = await _dataBase.ExecuteSqlQueryReturnParamString(query);

                //Количество купленных монет
                query = $@" SELECT SUM(balChg)
                      FROM `bills_table`
                      where `type`= '2' and ccy = '{coin}'  and balChg>'0';";

                tempReport.BuyAmount = await _dataBase.ExecuteSqlQueryReturnParamString(query);

                //На сумму в USDT
                query = $@" SELECT abs(SUM(balChg-fee))
                        FROM `bills_table`
                        where `type`= ""2"" and ccy = 'usdt' and
                               instId = '{coin}'""-USDT"" and balChg<'0';";
                //Console.WriteLine(query);
                tempReport.BuyTotal = await _dataBase.ExecuteSqlQueryReturnParamString(query);

                //Средняя цена покупки

                if (!string.IsNullOrWhiteSpace(tempReport.BuyAmount) &&
                    !string.IsNullOrWhiteSpace(tempReport.BuyTotal))
                {
                    Console.WriteLine("tempReport.BuyAmount = " + tempReport.BuyAmount);
                    Console.WriteLine("tempReport.BuyTotal = " + tempReport.BuyTotal);
                    tempReport.BuyAvgPrice = Convert.ToString(
                        Convert.ToDecimal(tempReport.BuyTotal) /
                        Convert.ToDecimal(tempReport.BuyAmount));
                }

                //Количество проданных монет
                query = $@" SELECT abs(SUM(balChg))
                      FROM `bills_table`
                      where `type`= '2' and ccy = '{coin}'  and balChg<'0';";

                tempReport.SellAmount = await _dataBase.ExecuteSqlQueryReturnParamString(query);

                //На сумму в USDT
                query = $@" SELECT abs(SUM(balChg))
                        FROM `bills_table`
                        where `type`= ""2"" and ccy = 'usdt' and
                               instId = '{coin}'""-USDT"" and balChg>'0';";

                tempReport.SellTotal = await _dataBase.ExecuteSqlQueryReturnParamString(query);

                //Средняя цена продажи
                if (!string.IsNullOrWhiteSpace(tempReport.SellAmount) &&
                    !string.IsNullOrWhiteSpace(tempReport.SellTotal))
                {
                    Console.WriteLine("tempReport.SellAmount = " + tempReport.SellAmount);
                    Console.WriteLine("tempReport.SellTotal = " + tempReport.SellTotal);
                    tempReport.SellAvgPrice = Convert.ToString(
                        Convert.ToDecimal(tempReport.SellTotal) /
                        Convert.ToDecimal(tempReport.SellAmount));
                }

                //% дохода
                if (!string.IsNullOrWhiteSpace(tempReport.BuyAvgPrice) &&
                   !string.IsNullOrWhiteSpace(tempReport.SellAvgPrice))
                {
                    var buyAvgPrice = Convert.ToDecimal(tempReport.BuyAvgPrice);
                    var sellAvgPrice = Convert.ToDecimal(tempReport.SellAvgPrice);
                    tempReport.ProfitPercent = Convert.ToString(
                        (sellAvgPrice - buyAvgPrice) / buyAvgPrice * 100);
                }

                //Монет в наличии 
                query = $@" SELECT SUM(balChg)
                        FROM `bills_table`
                        where ccy = '{coin}';";
                //Console.WriteLine(query);
                tempReport.CurrentAmount = await _dataBase.ExecuteSqlQueryReturnParamString(query);

                report.Add(tempReport);
                counter++;
            }
            return report;
        }

        //Сохранить отчет
        public async Task GenerateReportAsync(IEnumerable<OkxReport> data)
        {
            await using FileStream stream =
                File.Create("C:\\Users\\Djon\\source\\repos\\bot_analysis\\для теста\\Report.json");
            await JsonSerializer.SerializeAsync(stream, data, _jsonOptions);
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
                //ограничение скорости вызова запроса 5 запросов в 1 секунды
                await RateLimiter.EnforceRateLimit(5);

                if (string.IsNullOrEmpty(pointRead) && counter == 0)
                {
                    //если выполнились условия то это первый запрос
                    pageData = await _apiClient.GetApiDataAsync<ApiOkxBill, OkxBill>
                                (OkxUrlConst.Bill);
                    startedOver = true;
                }
                else
                {
                    if (startedOver)
                    {
                        //считываем данные от новых к старым
                        pageData = await _apiClient.GetApiDataAsync<ApiOkxBill, OkxBill>
                                (OkxUrlConst.Bill,
                                PaginationDirection.After, pointRead);
                    }
                    else
                    {
                        //считываем данные от станых к новым
                        pageData = await _apiClient.GetApiDataAsync<ApiOkxBill, OkxBill>
                                (OkxUrlConst.Bill,
                                PaginationDirection.Before, pointRead);
                    }
                }

                //сохраняем полученные данные в БД
                await _dataBase.SavePageAccountTransfersToDataBase(pageData);
                //достигнут конечный результат

                if (pageData.Count() < 100)
                {
                    len += pageData.Count();
                    _logger.Info("Закончили запрос транзакциям аккаунта. " +
                        "      Обработано " + len + " записей");
                    break;
                }

                len += pageData.Count();
                if (startedOver)
                    pointRead = pageData.LastOrDefault()?.BillId ?? "";
                else
                    pointRead = pageData.FirstOrDefault()?.BillId ?? "";
                _logger.Info("Обработано " + len + " записей");
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
                //ограничение скорости вызова запроса 5 запросов в 1 секунды
                await RateLimiter.EnforceRateLimit(5);

                if (string.IsNullOrEmpty(pointRead) && counter == 0)
                {
                    //если выполнились условия то это первый запрос
                    pageData = await _apiClient.GetApiDataAsync<ApiOkxTradeFillsHistory, OkxTradeFillsHistory>
                                (OkxUrlConst.FillHistorySpot);
                    startedOver = true;
                }
                else
                {
                    if (startedOver)
                    {
                        //считываем данные от новых к старым
                        pageData = await _apiClient.GetApiDataAsync<ApiOkxTradeFillsHistory, OkxTradeFillsHistory>
                                (OkxUrlConst.FillHistorySpot,
                                PaginationDirection.After, pointRead);
                    }
                    else
                    {
                        //считываем данные от старых к новым
                        pageData = await _apiClient.GetApiDataAsync<ApiOkxTradeFillsHistory, OkxTradeFillsHistory>
                                (OkxUrlConst.FillHistorySpot,
                                PaginationDirection.Before, pointRead);
                    }
                }

                //сохраняем полученные данные в БД
                await _dataBase.SavePageTradeFillsHistoryToDataBase(pageData);
                //достигнут конечный результат

                if (pageData.Count() < 100)
                {
                    len += pageData.Count();
                    _logger.Info("Закончили запрос транзакциям аккаунта. " +
                        "      Обработано " + len + " записей");
                    break;
                }

                len += pageData.Count();
                if (startedOver)
                    pointRead = pageData.LastOrDefault()?.billId ?? "";
                else
                    pointRead = pageData.FirstOrDefault()?.billId ?? "";
                _logger.Info("Обработано " + len + " записей");
                counter++;
            }
            while (true);
        }
    }
}
