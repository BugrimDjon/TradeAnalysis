using Google.Protobuf.WellKnownTypes;
using Mysqlx.Crud;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using bot_analysis.SQL;
using bot_analysis.Models;
using bot_analysis.App;

namespace bot_analysis.API
{
    public class API
    {
        // HTTP-клиент используется повторно для всех запросов
        private static readonly HttpClient client = new HttpClient();

        // Перечисление направлений запроса истории — "после" или "до" указанного ID
        public enum AfterBefore
        { 
            After,
            Before
        }

        // Метод создания HMAC-SHA256 подписи (используется для авторизации на OKX)
        private static string Sign(string message, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            return Convert.ToBase64String(hash); // Возвращает подпись в base64
        }

        public class GridBotOrdersResponse
        {   
            public string Code { get; set; }
            public string Msg { get; set; }
            public List<bot_orders> Data { get; set; }
        }





        

        public static Task<string> GetOrdersBotHistoryAsync(string algoId)
        {
            string urlPath;
        
                    urlPath = $"/api/v5/tradingBot/grid/sub-orders?algoOrdType=grid&algoId={algoId}&type=filled";
            
            return Get_Async(urlPath);
        }

        public static Task<string> GetOrdersBotHistoryAsync(string algoId, AfterBefore direction, string OrdId)
        {
            string urlPath;
            // Определяем путь запроса в зависимости от направления
            switch (direction)
            {
                case AfterBefore.After:
                    urlPath = $"/api/v5/tradingBot/grid/sub-orders?algoOrdType=grid&algoId={algoId}&type=filled&after={OrdId}";
                    break;
                case AfterBefore.Before:
                    urlPath = $"/api/v5/tradingBot/grid/sub-orders?algoOrdType=grid&algoId={algoId}&type=filled&before={OrdId}";
                    break;
                default:
                    throw new ArgumentException("Invalid direction."); // Обработка невалидного значения
            }

            return Get_Async(urlPath);
        }





        // Получение истории грид-ботов с указанием направления (после/до) и ID
        public static Task<string> GetGridBotHistoryAsync(AfterBefore direction, string algoId)
        {
            string urlPath;

            // Определяем путь запроса в зависимости от направления
            switch (direction)
            {
                case AfterBefore.After:
                    urlPath = $"/api/v5/tradingBot/grid/orders-algo-history?algoOrdType=grid&after={algoId}";
                    break;
                case AfterBefore.Before:
                    urlPath = $"/api/v5/tradingBot/grid/orders-algo-history?algoOrdType=grid&before={algoId}";
                    break;
                default:
                    throw new ArgumentException("Invalid direction."); // Обработка невалидного значения
            }

            return Get_Async(urlPath);
        }

        // Общий метод отправки GET-запроса на сервер OKX
        public static async Task<string> Get_Async(string urlPath)
        {
            string method = "GET";
            string body = ""; // Тело пустое для GET-запроса
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            // Формируем строку для подписи
            string prehash = timestamp + method + urlPath + body;
            string sign = Sign(prehash, AppAll.AppApiOKX.SecretKey);
            
            string url = "https://www.okx.com" + urlPath;
            //Console.WriteLine("Посылаем запрос: " + urlPath);
            

            // Очищаем и устанавливаем заголовки для авторизации запроса
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("OK-ACCESS-KEY", AppAll.AppApiOKX.ApiKey);
            client.DefaultRequestHeaders.Add("OK-ACCESS-SIGN", sign);
            client.DefaultRequestHeaders.Add("OK-ACCESS-TIMESTAMP", timestamp);
            client.DefaultRequestHeaders.Add("OK-ACCESS-PASSPHRASE", AppAll.AppApiOKX.Passphrase);

            try
            {
                // Выполняем HTTP GET-запрос
                HttpResponseMessage response = await client.GetAsync(url);

                // Проверяем статус и читаем ответ
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                // Обработка ошибок запроса
                Console.WriteLine($"Ошибка: {ex.Message}");
                return null;
            }
        }



        public static async Task SyncBotOrdersAsync(string algoId, string connectionString, bool? ollOrderId=true)
        {
            //производим считывание сделок бота по ID указанного в algoId
            //если ollOrderId = true то производим считывание сделок с нуля, не учитывая
            //считанные сделки до этого. Такая ситуация может быть необходима когда бот закончил
            //свою работу и поменял с "State = running" на "State = stopped", так образовался 
            //новый остановленный бот, для которого перезаписываем все сделки что бы избежать
            //неучтенных сделок после чего установиться "IsProcessed = 1" и на этом основании сделки
            //больше считываться не будут. Так как считывание данных от последней сделки ведет себя
            //не корректно, биржа выдает сделки, в том числе, которые уже были. Почему так происходит -
            //не разобрался, боюсь что можно какую то и пропустить. А неучтенные сделки по
            //работающему боту - не так критичны, все равно будут перезаписываться (обновляться) с нуля
            //когда будут остановленны.


            int x = 0;
            string lastOrderId=null;

            // Шаг 1: Получаем ID последней обработанной сделки из базы данных
            /*string query = @"SELECT ordId FROM bot_orders WHERE algoId = @"+algoId+" ORDER BY CAST(cTime AS UNSIGNED) DESC LIMIT 1";
            string lastOrderId = await bot_analysis.SQL.Database.GetValueFromDbAsync( connectionString, query);*/

            if (!ollOrderId.HasValue || !ollOrderId.Value)
                lastOrderId = await bot_analysis.SQL.Database.GetLastOrdIdFromDbAsync(algoId, connectionString);

            bool hasMore = true;
            string after = lastOrderId;

            while (hasMore)
            {
                // Шаг 2: Делаем запрос к API OKX для получения суб-ордеров (сделок)
                string json;
                if (string.IsNullOrEmpty(after))
                {
                    json = await GetOrdersBotHistoryAsync(algoId); // первый запрос
                    /*Console.WriteLine("Проход - " + x.ToString());
                    Console.WriteLine("Ответ:");
                    Console.WriteLine(json);*/
                }
                else
                {
                    json = await GetOrdersBotHistoryAsync(algoId, AfterBefore.After, after); // последующие
                    /*Console.WriteLine("Проход - " + x.ToString());
                    Console.WriteLine("Ответ:");
                    Console.WriteLine(json);*/
                }
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var response = JsonSerializer.Deserialize<GridBotOrdersResponse>(json, options);

                if (response?.Data == null || response.Data.Count == 0)
                {
                    hasMore = false; // Если новых данных нет — выходим из цикла
                    break;
                }

                // Шаг 3: Обрабатываем и сохраняем каждую сделку в базу данных
                foreach (var order in response.Data)
                {
                    await bot_analysis.SQL.Database.InsertOrderAsync(order, connectionString); // реализация ниже
                    
                    after = order.ordId; // сохраняем последнее значение ordId для следующего запроса
                }
                    //количество возвращаемых сделок должно быть 100, если меньше, то это были последние сделка
                if (response.Data.Count < 100)
                {
                    break;
                }
                    x++;
            }
        }


    }
}
