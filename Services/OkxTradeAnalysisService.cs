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





        /// <summary>
        /// Метод для обновления переводов на/с торгового счета
        /// </summary>
        /// <returns> нет возвращаемых параметров </returns>
        public async Task UpdateAccountTransfers()
        {
            int len = 0;
            int counter = 0;
            string pointRead = "";
            bool firstFill = false;

            IEnumerable<Bill> trades;

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
                    if (firstFill)
                    //считываем данные от новых к старым
                    trades = await _apiClient.GetTransfersStateAsync(PaginationDirection.After, pointRead.ToString());
                else
                    //считываем данные от старых к новым
                    trades = await _apiClient.GetTransfersStateAsync(PaginationDirection.Before, pointRead.ToString());

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

            IEnumerable<TradeFillsHistory> trades;

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
                    trades = await _apiClient.GetTradesAsync();
                else
                    trades = await _apiClient.GetTradesAsync(PaginationDirection.Before, lastTrade);

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
