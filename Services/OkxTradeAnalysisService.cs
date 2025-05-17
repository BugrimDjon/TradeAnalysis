// Services/OkxTradeAnalysisService.cs
using bot_analysis.Enums;
using bot_analysis.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using bot_analysis.Models;
using bot_analysis.Config;
using MySql.Data.MySqlClient;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Drawing;
using System.Text.Json;
using Mysqlx.Crud;
//using bot_analysis.Services;

namespace bot_analysis.Services
{
    /// <summary>
    /// Анализ сделок по биржи ОКХ
    /// </summary>
    public class OkxTradeAnalysisService : ITradeAnalysisService
    {
        private readonly ITradeApiClient _apiClient;
        private readonly IWorkWithDataBase _dataBase;

        //конструктор класса
        public OkxTradeAnalysisService(ITradeApiClient apiClient, string config)
        {
            _apiClient = apiClient;
            // Создаем подключение к БД из строки подключения
            var mySqlConnection = new MySqlConnection(config);
            _dataBase = new OkxWorkWithDataBase(mySqlConnection);
        }


        
        public async Task UpdateBotsAsync()
        {
            

            var bots = await _apiClient.GetStoppedBotsAsync();
        }







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
                //Console.WriteLine(query);
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
            var options = new JsonSerializerOptions
            {
                WriteIndented = true // чтобы JSON был читаемый
            };

            using FileStream stream =
                File.Create("C:\\Users\\Djon\\source\\repos\\bot_analysis\\для теста\\Report.json");
            await JsonSerializer.SerializeAsync(stream, data, options);
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
            int len = 0;
            int counter = 0;
            string pointRead = "";
            bool firstFill = false;

            IEnumerable<OkxBill> trades;

            /*var mySqlDataBase = new MySqlConnection(AppDataBase.ConnectionStringForDB());
            IWorkWithDataBase okxWorkWithDataBase = new OkxWorkWithDataBase(mySqlDataBase);*/


            pointRead = await _dataBase.SearchPointToReadNewDataForAccountTransfers();

            //lastTrade = "";

            do
            {
                //ограничение скорости вызова запроса 5 запросов в 2 секунды
                await RateLimiter.EnforceRateLimit(2);

                if ((counter == 0) && string.IsNullOrEmpty(pointRead))
                {
                    //если выполнились условия то будем считать что таблица пуста
                    // будем вычитывать все данные от новых к старым
                    trades = await _apiClient.GetTransfersStateAsync();
                    firstFill = true; //учитывает направление считывания от новых к старым
                }
                else
                {
                    if (firstFill)
                        //считываем данные от новых к старым
                        trades = await _apiClient.GetTransfersStateAsync(PaginationDirection.After, pointRead.ToString());
                    else
                        //считываем данные от старых к новым
                        trades = await _apiClient.GetTransfersStateAsync(PaginationDirection.Before, pointRead.ToString());
                }
                len += trades.Count();

                if (trades.Count() == 0)
                {
                    Console.WriteLine("Считано " + len + " сделок");
                    return;
                }

                await _dataBase.SavePageAccountTransfersToDataBase(trades);

                Console.WriteLine($"Получено сделок: {len}");

                pointRead = trades.LastOrDefault()?.BillId ?? "";


                if (len < 100)
                {
                    Console.WriteLine("Считано " + len + " сделок");
                    break;
                }
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
            int len = 0;
            int counter = 0;
            string lastTrade = "";
            bool firstFill = false;

            IEnumerable<OkxTradeFillsHistory> trades;

            /*var mySqlDataBase = new MySqlConnection(AppDataBase.ConnectionStringForDB());
            IWorkWithDataBase okxWorkWithDataBase = new OkxWorkWithDataBase(mySqlDataBase);*/

            //вычисляем последнюю сделку
            lastTrade = await _dataBase.SearcPointToReadNewDataForFillsHistory();
            //lastTrade = "";


            do
            {
                //ограничение скорости вызова запроса 20 запросов в секунду
                await RateLimiter.EnforceRateLimit(20);

                //запрос ручных сделок
                if ((counter == 0) && (lastTrade == ""))
                {
                    //если выполнились условия то будем считать что таблица пуста
                    // будем вычитывать все данные от новых к старым
                    trades = await _apiClient.GetTradesAsync();
                    firstFill = true; //учитывает направление считывания от новых к старым
                }

                else
                {

                    if (firstFill)
                        //считываем данные от новых к старым
                        trades = await _apiClient.GetTradesAsync(PaginationDirection.After, lastTrade);
                    else
                        //считываем данные от старых к новым
                        trades = await _apiClient.GetTradesAsync(PaginationDirection.Before, lastTrade);
                }




                len += trades.Count();

                if (trades.Count() == 0)
                {
                    Console.WriteLine("Считано " + len + " сделок");
                    return;
                }

                await _dataBase.SavePageTradeFillsHistoryToDataBase(trades);

                Console.WriteLine($"Получено сделок: {len}");

                lastTrade = trades.LastOrDefault()?.billId ?? "";

                if (len < 100)
                {
                    Console.WriteLine("Считано " + len + " сделок");
                    break;
                }
                counter++;
            }
            while (true);
        }



    }
}
