using bot_analysis.App;
using bot_analysis.Enums;
using bot_analysis.Interfaces;
using DotNetEnv;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using bot_analysis.Config;
using System.Security.Cryptography;
using bot_analysis.Models;
using static bot_analysis.API.API;
using Mysqlx.Crud;
using Org.BouncyCastle.Utilities.Collections;
using System.IO;
using System.Reflection.Metadata;
//using bot_analysis.Response;

namespace bot_analysis.Services
{
    internal class OkxApiClient : ITradeApiClient
    {
        private readonly HttpClient _httpClient;

        public OkxApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        
        /// <summary>
        /// метод для запроса информации по остановленным ботам 
        /// </summary>
        /// <returns> возвращает распарсеный JSON в виде списка List<OkxBot></returns>

        public async Task<IEnumerable<OkxBot>> GetStoppedBotsAsync(PaginationDirection? afterBefore = null, string? billId = null)
        {

            //urlPath = $"/api/v5/asset/transfer-state?transId=0";
            //urlPath = "/api/v5/asset/deposit-history";
            //urlPath = "/api/v5/asset/withdrawal-history";

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Получаем одну страницу JSON ответа 
            string botStoppingJson = await GetBotStoppingJson(afterBefore, billId);

            if (string.IsNullOrEmpty(botStoppingJson))
            {
                Console.WriteLine("Не удалось получить данные.");
                return new List<OkxBot>(); // Возвращаем пустой список, если нет данных
            }

            Console.WriteLine(botStoppingJson);
            // Десериализация ответа в объект ApiTrade
            var result = JsonSerializer.Deserialize<ApiOkxBot>(botStoppingJson, options);

            // Возвращаем список сделок, если он существует, иначе пустой список
            return result?.data ?? new List<OkxBot>();




        }






        /// <summary>
        /// метод для запроса сделок выполненых вручную
        /// </summary>
        /// <returns> возвращает распарсеный JSON в виде списка List<TradeFillsHistory></returns>
        public async Task<IEnumerable<OkxTradeFillsHistory>> GetTradesAsync(PaginationDirection? afterBefore = null, string? billId = null)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Получаем одну страницу JSON ответа по сделкам
            string transactionsSpotJson = await GetTradeFillHistorySpotOnePageJson(afterBefore, billId);

            if (string.IsNullOrEmpty(transactionsSpotJson))
            {
                Console.WriteLine("Не удалось получить данные.");
                return new List<OkxTradeFillsHistory>(); // Возвращаем пустой список, если нет данных
            }

            // Десериализация ответа в объект ApiTrade
            var result = JsonSerializer.Deserialize<ApiOkxTradeFillsHistory>(transactionsSpotJson, options);

            // Возвращаем список сделок, если он существует, иначе пустой список
            return result?.data ?? new List<OkxTradeFillsHistory>();
        }


        /// <summary>
        /// Возвращает DJSON по остановленным ботам
        /// </summary>
        /// <param name="afterBefore">  указывает направление пагинации 
        ///            принимает значение PaginationDirection.After или
        ///            PaginationDirection.Before     </param>
        ///<param name="billId">указывает с какого billId начинать пагинацию </param>   
        private async Task<string> GetBotStoppingJson(PaginationDirection? afterBefore = null, string? billId = null)
        {
            string urlPath;
            
            
            if (afterBefore == null && billId == null)
            {           
                urlPath = "/api/v5/tradingBot/grid/orders-algo-history?algoOrdType=grid&limit=100";

            }
            else if (afterBefore != null && billId != null)
            {
                switch (afterBefore)
                {
                    case PaginationDirection.After:
                        urlPath = $"/api/v5/tradingBot/grid/orders-algo-history?algoOrdType=grid&limit=100&after={billId}";
                        break;
                    case PaginationDirection.Before:
                        urlPath = $"/api/v5/tradingBot/grid/orders-algo-history?algoOrdType=grid&limit=100&before={billId}";
                        break;
                    default:
                        throw new ArgumentException("Invalid direction.");
                }
            }
            else
            {
                throw new ArgumentException("Необходимо указать оба параметра: direction и algoId.");
            }

            Console.WriteLine(urlPath);
            return await GetJsonAsyncByUrlPath(urlPath);
        }







        /// <summary>
        /// Возвращает DJSON по спотовым сделкам
        /// </summary>
        /// <param name="afterBefore">  указывает направление пагинации 
        ///            принимает значение PaginationDirection.After или
        ///            PaginationDirection.Before     </param>
        ///<param name="billId">указывает с какого billId начинать пагинацию </param>   
        private async Task<string> GetTradeFillHistorySpotOnePageJson(PaginationDirection? afterBefore = null, string? billId = null)
        {
            string urlPath;

            if (afterBefore == null && billId == null)
            {
                urlPath = "/api/v5/trade/fills-history?instType=SPOT&limit=100";

            }
            else if (afterBefore != null && billId != null)
            {
                switch (afterBefore)
                {
                    case PaginationDirection.After:
                        urlPath = $"/api/v5/trade/fills-history?instType=SPOT&limit=100&after={billId}";
                        break;
                    case PaginationDirection.Before:
                        urlPath = $"/api/v5/trade/fills-history?instType=SPOT&limit=100&before={billId}";
                        break;
                    default:
                        throw new ArgumentException("Invalid direction.");
                }
            }
            else
            {
                throw new ArgumentException("Необходимо указать оба параметра: direction и algoId.");
            }

            return await GetJsonAsyncByUrlPath(urlPath);
        }


        private static string Sign(string message, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            return Convert.ToBase64String(hash); // Возвращает подпись в base64
        }


        private async Task<string> GetJsonAsyncByUrlPath(string urlPath)
        {
            string method = "GET";
            string body = ""; // Тело пустое для GET-запроса
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ssZ");

            // Формируем строку для подписи
            string prehash = timestamp + method + urlPath + body;
            string sign = Sign(prehash, AppDataApiOKX.SecretKey);

            string url = "https://www.okx.com" + urlPath;
            Console.WriteLine("Посылаем запрос: " + urlPath);


            // Очищаем и устанавливаем заголовки для авторизации запроса
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("OK-ACCESS-KEY", AppDataApiOKX.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("OK-ACCESS-SIGN", sign);
            _httpClient.DefaultRequestHeaders.Add("OK-ACCESS-TIMESTAMP", timestamp);
            _httpClient.DefaultRequestHeaders.Add("OK-ACCESS-PASSPHRASE", AppDataApiOKX.Passphrase);

            try
            {
                // Выполняем HTTP GET-запрос
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                // Проверяем статус и читаем ответ
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                // Обработка ошибок запроса
                Console.WriteLine($"Ошибка: {ex.Message}");
                return "";
            }
        }

        

        public async Task<string> GetTransfersStateAsyncOnePageJson(PaginationDirection? afterBefore = null, string? billId = null)
        {
            string urlPath;

            //            GET https://www.okx.com/api/v5/asset/transfer-state?ccy=USDT&type=0&limit=100

            



            Console.WriteLine("Производим считывание таблицы bill");

            

            if (string.IsNullOrEmpty(billId) && afterBefore == null)
                urlPath = "/api/v5/account/bills-archive";
            else
            {
                if (billId != "" && afterBefore != null)
                {

                    switch (afterBefore)
                    {
                        case PaginationDirection.After:
                            urlPath = $"/api/v5/account/bills-archive?after={billId}";
                            break;
                        case PaginationDirection.Before:
                            urlPath = $"/api/v5/account/bills-archive?before={billId}";
                            break;
                        default:
                            throw new ArgumentException("Invalid direction.");
                            

                    }
                }
                else
                {
                    throw new ArgumentException("Необходимо указать оба параметра: afterBefore и billId.");
                }

            }

            return (await GetJsonAsyncByUrlPath(urlPath));

            //File.WriteAllText("C:\\Users\\Djon\\source\\repos\\bot_analysis\\для теста\\123.json", urlPath);
            //Console.ReadLine();
        }



        /// <summary>
        /// метод для запроса переводов на счет
        /// </summary>
        /// <returns> возвращает распарсеный JSON в виде списка List<Bill></returns>
        public async Task<IEnumerable<OkxBill>> GetTransfersStateAsync(PaginationDirection? afterBefore = null, string? billId = null)
        {
            

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Получаем одну страницу JSON ответа
            string dataJson = await GetTransfersStateAsyncOnePageJson(afterBefore, billId);

            if (string.IsNullOrEmpty(dataJson))
            {
                Console.WriteLine("Не удалось получить данные.");
                return new List<OkxBill>(); // Возвращаем пустой список, если нет данных
            }

            // Десериализация ответа в объект ApiTrade
            var result = JsonSerializer.Deserialize<ApiBill>(dataJson, options);

            // Возвращаем список сделок, если он существует, иначе пустой список
            return result?.data ?? new List<OkxBill>();

        }

    }
}
