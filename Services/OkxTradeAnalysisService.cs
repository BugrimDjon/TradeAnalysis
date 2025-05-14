// Services/OkxTradeAnalysisService.cs
using bot_analysis.Enums;
using bot_analysis.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using bot_analysis.Models;
using bot_analysis.Config;
using MySql.Data.MySqlClient;

namespace bot_analysis.Services
{
    /// <summary>
    /// Анализ сделок по биржи ОКХ
    /// </summary>
    public class OkxTradeAnalysisService : ITradeAnalysisService
    {
        private readonly ITradeApiClient _apiClient;

        //конструктор класса
        public OkxTradeAnalysisService(ITradeApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        /// <summary>
        /// Метод для анализа ручных сделок 
        /// </summary>
        /// <returns> нет возвращаемых параметров </returns>
        public async Task AnalyzeTradesAsync()
        {
            int len = 0;
            int counter = 0;
            string lastTrade = "";
            IEnumerable<TradeFillsHistory> trades;

            var mySqlDataBase = new MySqlConnection(AppDataBase.ConnectionStringForDB());
            IWorkWithDataBase okxWorkWithDataBase = new OkxWorkWithDataBase(mySqlDataBase);

            lastTrade = await okxWorkWithDataBase.SearchLastTradeFillsHistoryFromDB();
            //lastTrade = "";


            do
            {

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

                await okxWorkWithDataBase.SaveTradeFillsHistoryToDataBase(trades);

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
