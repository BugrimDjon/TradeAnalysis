using bot_analysis.Config.OKX;
using bot_analysis.Enums;
using bot_analysis.Interfaces;
using bot_analysis.Models.OKX;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace bot_analysis.Services.OKX
{
    public class OkxApiClient : ITradeApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger _logger;

        public OkxApiClient(HttpClient httpClient, JsonSerializerOptions jsonOptions, ILogger logger)
        {
            _httpClient = httpClient;
            _jsonOptions = jsonOptions;
            _logger = logger;
        }

        



        /// <summary>
        /// универсальный метод для запроса по API
        /// </summary>
        /// <returns> возвращает распарсеный JSON в виде списка List<TData></returns>
        public async Task<IEnumerable<TData>> GetApiDataAsync<TResponse, TData>
                                        (OkxEndpointInfo endPointData,
                                        PaginationDirection? afterBefore = null,
                                        string? pointRead = null)
            where TResponse : IApiResponseWithData<TData>

        {
            //ограничение скорости вызова запроса
            await RateLimiter.EnforceRateLimit(endPointData.Frequency);
            // Получаем одну страницу JSON ответа по сделкам
            string responseJson = await GetPageJsonAsync(endPointData.Url, afterBefore, pointRead);
            _logger.Info(responseJson);
            //_logger.Debug(responseJson);
            if (string.IsNullOrEmpty(responseJson))
            {
                Console.WriteLine("Не удалось получить данные.");
                return new List<TData>(); // Возвращаем пустой список, если нет данных
            }

            // Десериализация ответа в объект ApiTrade
            var result = JsonSerializer.Deserialize<TResponse>(responseJson, _jsonOptions);

            // Возвращаем список сделок, если он существует, иначе пустой список
            return result?.data ?? new List<TData>();
        }

        /// <summary>
        /// универсальный метод для запроса по API
        /// </summary>
        /// <returns> возвращает распарсеный JSON в виде списка Task<DataTable> </returns>
        public async Task<DataTable> GetApiDataAsDataTableUniversalAsync(
                                OkxEndpointInfo endPointData,
                                PaginationDirection? afterBefore = null,
                                string? pointRead = null)
        {
            //ограничение скорости вызова запроса
            await RateLimiter.EnforceRateLimit(endPointData.Frequency);

            string responseJson = await GetPageJsonAsync(endPointData.Url, afterBefore, pointRead);
            _logger.Info(responseJson);

            var table = new DataTable();

            if (string.IsNullOrEmpty(responseJson))
            {
                Console.WriteLine("Нет данных.");
                return table;
            }

            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            if (!root.TryGetProperty("data", out var dataElement))
            {
                Console.WriteLine("Не найдено свойство 'data'.");
                return table;
            }

            // Массив объектов [{...}, {...}]
            if (dataElement.ValueKind == JsonValueKind.Array && dataElement.GetArrayLength() > 0)
            {
                var firstRow = dataElement[0];
                if (firstRow.ValueKind == JsonValueKind.Object)
                {
                    // Создаём столбцы
                    foreach (var prop in firstRow.EnumerateObject())
                    {
                        table.Columns.Add(prop.Name, typeof(string)); // можно задать другой тип, если нужно
                    }

                    // Заполняем строки
                    foreach (var rowElement in dataElement.EnumerateArray())
                    {
                        var row = table.NewRow();
                        foreach (var prop in rowElement.EnumerateObject())
                        {
                            row[prop.Name] = prop.Value.ToString();
                        }
                        table.Rows.Add(row);
                    }

                    return table;
                }

                // Массив массивов [["...", "..."], ["...", "..."]]
                if (firstRow.ValueKind == JsonValueKind.Array)
                {
                    int colCount = firstRow.GetArrayLength();
                    for (int i = 0; i < colCount; i++)
                    {
                        table.Columns.Add($"col{i + 1}", typeof(string));
                    }

                    foreach (var arrayRow in dataElement.EnumerateArray())
                    {
                        var row = table.NewRow();
                        for (int i = 0; i < arrayRow.GetArrayLength(); i++)
                        {
                            row[i] = arrayRow[i].ToString();
                        }
                        table.Rows.Add(row);
                    }

                    return table;
                }
            }

            Console.WriteLine("Формат 'data' не поддерживается.");
            return table;
        }





        /// <summary>
        /// метод для запроса сделок выполненых ботами
        /// </summary>
        /// <returns> возвращает распарсеный JSON в виде списка List<ApiOkxBot></returns>
        public async Task<IEnumerable<OkxBotOrder>> GetOrdesBotAsync
                                        (string OkxUrl,
                                        PaginationDirection? afterBefore = null,
                                        string? pointRead = null)

        {
            // Получаем одну страницу JSON ответа по сделкам
            string responseJson = await GetPageJsonAsync(OkxUrl, afterBefore, pointRead);

            if (string.IsNullOrEmpty(responseJson))
            {
                Console.WriteLine("Не удалось получить данные.");
                return new List<OkxBotOrder>(); // Возвращаем пустой список, если нет данных
            }

            // Десериализация ответа в объект ApiTrade
            var result = JsonSerializer.Deserialize<ApiOkxBotOrder>(responseJson, _jsonOptions);

            // Возвращаем список сделок, если он существует, иначе пустой список
            return result?.data ?? new List<OkxBotOrder>();
        }

        public async Task<string> GetPageJsonAsync(string urlPath,
                                                   PaginationDirection? direction = null,
                                                   string? point = null)
        {
            if (direction != null && !string.IsNullOrEmpty(point))
            {
                string param = direction switch
                {
                    PaginationDirection.After => $"&after={point}",
                    PaginationDirection.Before => $"&before={point}",
                    _ => throw new ArgumentException("Некорректное направление пагинации")
                };

                urlPath += param;
            }

            _logger.Debug($"Запрос URL: {urlPath}");
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
            const string method = "GET";
            string body = ""; // Тело пустое для GET-запроса
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ssZ");

            // Формируем строку для подписи
            string prehash = timestamp + method + urlPath + body;
            string sign = Sign(prehash, AppOkxDataApi.SecretKey);

            string url = "https://www.okx.com" + urlPath;
            //Console.WriteLine("Посылаем запрос: " + urlPath);

            // Очищаем и устанавливаем заголовки для авторизации запроса
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("OK-ACCESS-KEY", AppOkxDataApi.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("OK-ACCESS-SIGN", sign);
            _httpClient.DefaultRequestHeaders.Add("OK-ACCESS-TIMESTAMP", timestamp);
            _httpClient.DefaultRequestHeaders.Add("OK-ACCESS-PASSPHRASE", AppOkxDataApi.Passphrase);

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

            _logger.Debug("Производим считывание таблицы bill");

            if (string.IsNullOrEmpty(billId) && afterBefore == null)
            {
                urlPath = "/api/v5/account/bills-archive";
            }
            else
            {
                if (billId != "" && afterBefore != null)
                {
                    urlPath = afterBefore switch
                    {
                        PaginationDirection.After =>
                            $"/api/v5/account/bills-archive?limit=5&after={billId}",
                        PaginationDirection.Before =>
                            $"/api/v5/account/bills-archive?limit=5&before={billId}",
                        _ => throw new ArgumentException("Invalid direction.")
                    };
                }
                else
                {
                    throw new ArgumentException("Необходимо указать оба параметра: afterBefore и billId.");
                }
            }
            _logger.Debug(urlPath);
            return await GetJsonAsyncByUrlPath(urlPath);

            //File.WriteAllText("C:\\Users\\Djon\\source\\repos\\bot_analysis\\для теста\\123.json", urlPath);
            //Console.ReadLine();
        }
    }
}
