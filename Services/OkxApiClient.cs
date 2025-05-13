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
        /// метод для запроса сделок выполненых вручную
        /// </summary>
        /// <returns> возвращает распарсеный JSON в виде списка List<TradeFillsHistory></returns>
        public async Task<IEnumerable<TradeFillsHistory>> GetTradesAsync(PaginationDirection? afterBefore = null, string? billId = null)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Получаем JSON ответ по сделкам
            string transactionsSpotJson = await GetTradesFillsHistorySpotJson(afterBefore, billId);
            
            if (string.IsNullOrEmpty(transactionsSpotJson))
            {
                Console.WriteLine("Не удалось получить данные.");
                return new List<TradeFillsHistory>(); // Возвращаем пустой список, если нет данных
            }

            // Десериализация ответа в объект ApiTrade
            var result = JsonSerializer.Deserialize<ApiTrade>(transactionsSpotJson, options);

            // Возвращаем список сделок, если он существует, иначе пустой список
            return result?.data ?? new List<TradeFillsHistory>();
        }



                    /// <summary>
                    /// Возвращает DJSON по спотовым сделкам
                    /// </summary>
                    /// <param name="afterBefore">  указывает направление пагинации 
                    ///            принимает значение PaginationDirection.After или
                    ///            PaginationDirection.Before     </param>
                    ///<param name="billId">указывает с какого billId начинать пагинацию </param>   
        private async Task<string> GetTradesFillsHistorySpotJson(PaginationDirection? afterBefore = null, string? billId = null)
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
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            // Формируем строку для подписи
            string prehash = timestamp + method + urlPath + body;
            string sign = Sign(prehash, AppDataApiOKX.SecretKey);

            string url = "https://www.okx.com" + urlPath;
            //Console.WriteLine("Посылаем запрос: " + urlPath);


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

    }
}
