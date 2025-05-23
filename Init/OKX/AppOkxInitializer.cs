using bot_analysis.Config;
using bot_analysis.Services.OKX;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using bot_analysis.Services;

using bot_analysis.Interfaces;

namespace bot_analysis.Init.OKX

{
    public static class AppOkxInitializer
    {
        public static (ITradeAnalysisService service, ILogger logger) Initialize()
        {
            var logger = new ConsoleLogger();
            var httpClient = new HttpClient();
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var apiClient = new OkxApiClient(httpClient, jsonOptions, logger);

            var connectionString = AppDataBase.ConnectionStringForDB();
            var db = new OkxWorkWithDataBase(new MySqlConnection(connectionString));
            var servise = new OkxTradeAnalysisService(apiClient, db, jsonOptions,  logger);

            return  (servise, logger);
        }
    }
}
