using bot_analysis.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bot_analysis.API; // подключаем пространство имён с классом API
using System.Text.Json;
using bot_analysis.Models; // для доступа к классам модели
using bot_analysis.SQL;
using Google.Protobuf.Collections;
using static bot_analysis.API.API;
using MySql.Data.MySqlClient;
using Google.Protobuf.WellKnownTypes;
using MySqlX.XDevAPI.Common;
using bot_analysis.Response;



namespace bot_analysis.from_API_to_database
{
    public class ApiToDatabase
    {
        public class TradeFillsHistory
        {
            public static async Task UpdateTransactionsSpot()
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                string transactionsSpotJson = await API.TradeFillsHistory.GetTradeFillsHistorySpot();

                if (!string.IsNullOrEmpty(transactionsSpotJson))
                {
                    //Console.WriteLine("История сделок:");
                    //Console.WriteLine(transactionsSpotJson);
                }
                else
                {
                    Console.WriteLine("Не удалось получить данные.");
                }


                var result = JsonSerializer.Deserialize<TradeFillsHistoryResponse>
                    (transactionsSpotJson, options);
                Console.WriteLine(result);



                //*****************************************************






            }

        }
        public static async Task UpdateTransactionsBots()
        {
            await UpdateTransactionsStoppedBots();
            await UpdateTransactionsRunningdBots();

        }

        public static async Task UpdateTransactionsRunningdBots()
        {

            int x = 0;
            // Производим обновление ордеров для работающих ботов 

            // Шаг 1: Получаем список AlgoId с состоянием "running"
            Console.WriteLine();
            Console.WriteLine("Производим обработку транзакций работающих ботов");
            var runningBots = await bot_analysis.SQL.GridBots.GetAlgoIdRunningAsync(AppAll.AppSql.GetConnectionString());

            foreach (var bot in runningBots)
            {
                await SyncBotOrdersAsync(bot, AppAll.AppSql.GetConnectionString(), false);
                x++;

            }

            Console.WriteLine("Обновлены транзпкции по " + x + "ботам");
            Console.WriteLine("------------------------------------------------------------------------------");

        }

        public static async Task UpdateTransactionsStoppedBots()
        {
            string number_bot;
            int x = 0;
            // Производим обновление ордеров для ботов закончивших работу
            Console.WriteLine();
            Console.WriteLine("Производим обработку транзакций остановленных ботов");
            do
            {
                // производим запрос такого бота, у которого "IdStopped = stopped", то есть остановлен
                // и "IsProcesse = False", то есть остановленным до этого не обрабатывался

                number_bot = await bot_analysis.SQL.GridBots.GetAlgoIdStoppedAndIsProcessedAsync(AppAll.AppSql.GetConnectionString());


                if (string.IsNullOrEmpty(number_bot))
                {
                    Console.WriteLine("Обновлены транзпкции по " + x + " ботам");
                    Console.WriteLine("------------------------------------------------------------------------------");
                    break;
                }

                await SyncBotOrdersAsync(number_bot, AppAll.AppSql.GetConnectionString());

                await bot_analysis.SQL.GridBots.SetIsProcessedForAlgoId(number_bot, AppAll.AppSql.GetConnectionString());
                x++;
            }
            while (true);
        }

        public static async Task UpdateBotList()
        {
            await UpdateBotListStoppedBto();
            await UpdateBotListRunningBto();
        }

        public static async Task UpdateBotListStoppedBto()
        {
            await UpdateAllBotList(true);
        }

        public static async Task UpdateBotListRunningBto()
        {
            await UpdateAllBotList(false);
        }


        public static async Task UpdateAllBotList(bool oldBot)
        {

            string botsHistoryJson;
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            int x = 0;
            int numberOfRecords = 0;
            int numberBots = 0;
            string oldBotId = null;


            if (oldBot)
            {
                Console.WriteLine("Запрашиваем информацию по завершённм ботам");
            }
            else
            {
                Console.WriteLine("Запрашиваем информацию по работающим ботам");
            }


            do
            {
                if (x == 0)
                {
                    //Запрос на биржу истории  ботоав
                    if (oldBot)
                        botsHistoryJson = await API.GridBots.GetGridBotHistoryAsync();
                    else
                        botsHistoryJson = await API.GridBots.GetGridBotActiveAsync();
                }
                else
                {
                    //Запрос на биржу истории  ботоав
                    if (oldBot)
                        botsHistoryJson = await API.GridBots.GetGridBotHistoryAsync(API.API.AfterBefore.After, oldBotId);
                    else
                        botsHistoryJson = await API.GridBots.GetGridBotActiveAsync(API.API.AfterBefore.After, oldBotId);
                }

                // проверка на наличие ответа
                if (!string.IsNullOrEmpty(botsHistoryJson))
                {
                    //Console.WriteLine("История ботов:");
                    //Console.WriteLine(botsHistoryJson);
                }
                else
                {
                    Console.WriteLine("Не удалось получить данные.");
                }
                //парсим ответ от биржи

                var result = JsonSerializer.Deserialize<GridBotResponse>(botsHistoryJson, options);
                //Console.WriteLine(result);
                numberBots += result.Data.Count;


                if (result?.Data.Count == null && x == 0)
                {
                    Console.WriteLine("Нет данных от запроса по закончившим работу ботам");
                    break;
                }
                else
                {
                    if (result?.Data.Count == null)
                    {
                        break;
                    }
                    foreach (var bot in result.Data)
                    {
                        //если вычитка работающих ботов,
                        //то занести информацтю про этого бота в таблицу
                        if (bot.AlgoId == "2466860489448620032")
                            Console.WriteLine(bot.AlgoId);
                        if (!oldBot)
                        {
                            await Database.InsertGridBotAsync(bot, AppAll.AppSql.GetConnectionString());
                            numberOfRecords++;
                        }
                        
                        else
                        {       //обработка остановленных ботов
                                //если такого AlgoId нет или его статус "running" (остановился с последнего опроса)
                                //занести информацтю про этого бота в таблицу
                            if (!await SQL.GridBots.SearchAlgoIdAsinc(bot.AlgoId) || 
                                await SQL.GridBots.GetStateByAlgoIdAsync(bot.AlgoId)== "running")
                            {
                                await Database.InsertGridBotAsync(bot, AppAll.AppSql.GetConnectionString());
                                numberOfRecords++;
                            }

                        }
                        oldBotId = bot.AlgoId;
                    }
                }
                x++;

                if (result.Data.Count < 100)
                {
                    break;
                }

            } while (true);

            Console.WriteLine("Всего ботов - " + numberBots);
            Console.WriteLine("Записано(обновлено) записей - " + numberOfRecords);
            Console.WriteLine("------------------------------------------------------------------------------");
        }
    }

}
