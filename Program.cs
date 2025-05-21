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
using bot_analysis.Interfaces;
using bot_analysis.Services;
using bot_analysis.Config;
using Org.BouncyCastle.X509.Store;

    
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
            /*//*************************НАЧАЛО Закоментирован блок обработки для тестов
            //произвести обновление списка остановленных и работающих ботов 
            await ApiToDatabase.UpdateBotList();
            
            //произвести обновление транзакций для неучтенных ботов которые закончили работу
            //и для всех работающих ботов
            await ApiToDatabase.UpdateTransactionsBots();

            //поиск и запись уникальных торговых пар из таблицы ботов
            await SQL.InstId.UpdateTradingPairsFromBots(AppAll.AppSql.GetConnectionString());


            
            await Results.PrintResultBotGpt(await Results.GetCoinStatsGroupedByInstIdAsync(AppAll.AppSql.GetConnectionString()));

            //await Results.PrintResultBotGpt();
           
            await ApiToDatabase.TradeFillsHistory.UpdateTransactionsSpot();
            
            //************************* КОНЕЦ Закоментирован блок обработки для тестов*/




            //*************************** Н А Ч А Л О     Н О В О Й  А Р Х И Т Е К Т У Р Ы
            
            // Создаем экземпляр HttpClient
            var httpClient = new HttpClient();

            Console.WriteLine(DateTime.UtcNow.ToUniversalTime());
            
                ILogger logger = new ConsoleLogger();
                ITradeApiClient okxApiClient = new OkxApiClient(httpClient, logger);
                ITradeAnalysisService tradeAnalysisService = 
                                    new OkxTradeAnalysisService(okxApiClient, AppDataBase.ConnectionStringForDB(), logger);
                

            logger.IsEnabled = true;

            //+await tradeAnalysisService.UpdateBotsAsync();   //Обновление информации по ботам

            await tradeAnalysisService.UpdateTradesAsync();//Обновление ручных сделок 
            //+await tradeAnalysisService.UpdateAccountTransfersAsync();//Обновление переводов на счет

            //await tradeAnalysisService.UpdateUniqueTradingPairsAsync();//обновить уникальные торговые пары
            //await tradeAnalysisService.UpdateUniqueCoinsAsync();//обновить уникальные монеты

            //сгенерировать и созранить отчет
            //await tradeAnalysisService.GenerateReportAsync(await tradeAnalysisService.GenerateReport());

            Console.WriteLine("Для выхода нажмите клавишу");
            Console.Read();
            
        }





    }
}
