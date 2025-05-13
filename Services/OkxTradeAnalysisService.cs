// Services/OkxTradeAnalysisService.cs
using bot_analysis.Enums;
using bot_analysis.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using bot_analysis.Models;

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
            int len;
            int counter = 0;
            string lastTrade="";

            do
            {
                IEnumerable<TradeFillsHistory> trades;
                //запрос ручных сделок
                if (counter == 0)
                    trades = await _apiClient.GetTradesAsync();
                else
                    trades = await _apiClient.GetTradesAsync(PaginationDirection.After, lastTrade);

                len = trades.Count();

                if (len == 0)
                {
                    Console.WriteLine("Нет доступных сделок для анализа.");
                    return;
                }

                Console.WriteLine($"Получено сделок: {trades.Count()}");
                foreach (var trade in trades)
                {
                    Console.WriteLine($"instId: {trade.instId}, цена: {trade.fillPx}, кол-во: {trade.fillSz}, side: {trade.side}");
                    lastTrade = trade.billId;
                }
                if (len < 100)
                {
                    Console.WriteLine("Все сделки считаны");
                    break;
                }
                counter++;
            }
            while (true);
        }
    }
}
