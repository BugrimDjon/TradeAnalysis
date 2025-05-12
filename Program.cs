using System;
using System.Threading.Tasks;
using bot_analysis.API; // подключаем пространство имён с классом API
using System.Text.Json;
using bot_analysis.Models; // для доступа к классам модели
using bot_analysis.SQL;
using bot_analysis.App;
using Google.Protobuf.Collections;
using static bot_analysis.API.API;
using MySql.Data.MySqlClient;
using bot_analysis.from_API_to_database;
using bot_analysis.TotalResults;
using MySqlX.XDevAPI.Common;


/*Получить активные боты	GET /api/v5/tradingBot/grid/orders-algo-pending
Получить завершённые (история) боты	GET /api/v5/tradingBot/grid/orders-algo-history
Получить ордера по конкретному боту	GET /api/v5/tradingBot/grid/sub-orders?algoId=...*/




namespace bot_analysis
{
    class Program
    {
        static async Task Main(string[] args)
        {


            // очистить экран
            Console.Clear();
            /*************************НАЧАЛО Закоментирован блок обработки ботов
            //произвести обновление списка остановленных и работающих ботов 
            await ApiToDatabase.UpdateBotList();
            
            //произвести обновление транзакций для неучтенных ботов которые закончили работу
            //и для всех работающих ботов
            await ApiToDatabase.UpdateTransactionsBots();

            //поиск и запись уникальных торговых пар из таблицы ботов
            await SQL.InstId.UpdateTradingPairsFromBots(AppSql.GetConnectionString());


            
            await Results.PrintResultBotGpt(await Results.GetCoinStatsGroupedByInstIdAsync(AppSql.GetConnectionString()));

            //await Results.PrintResultBotGpt();
            /************************* КОНЕЦ Закоментирован блок обработки ботов*/
           
            await ApiToDatabase.TradeFillsHistory.UpdateTransactionsSpot();
            


            Console.Read();
            
        }





    }
}
