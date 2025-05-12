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


//using bot_analysis.API;

namespace bot_analysis.API
{
    class GridBots
    {
        // Получение полной истории завершенных грид-ботов
        public static Task<string> GetGridBotHistoryAsync(API.AfterBefore? afterBefore = null, string? algoId = null)
        {
            string urlPath;

            if (afterBefore == null && algoId == null)
            {
                urlPath = "/api/v5/tradingBot/grid/orders-algo-history?algoOrdType=grid";
            }
            else if (afterBefore != null && algoId != null)
            {
                switch (afterBefore)
                {
                    case API.AfterBefore.After:
                        urlPath = $"/api/v5/tradingBot/grid/orders-algo-history?algoOrdType=grid&after={algoId}";
                        break;
                    case API.AfterBefore.Before:
                        urlPath = $"/api/v5/tradingBot/grid/orders-algo-history?algoOrdType=grid&before={algoId}";
                        break;
                    default:
                        throw new ArgumentException("Invalid direction.");
                }
            }
            else
            {
                throw new ArgumentException("Необходимо указать оба параметра: direction и algoId.");
            }

            return API.Get_Async(urlPath);
        }


        public static Task<string> GetGridBotActiveAsync(API.AfterBefore? afterBefore = null, string? algoId = null)
        {
            string urlPath;

            if (afterBefore == null && algoId == null)
            {
                urlPath = "/api/v5/tradingBot/grid/orders-algo-pending?algoOrdType=grid";
            }
            else if (afterBefore != null && algoId != null)
            {
                switch (afterBefore)
                {
                    case API.AfterBefore.After:
                        urlPath = $"/api/v5/tradingBot/grid/orders-algo-pending?algoOrdType=grid&after={algoId}";
                        break;
                    case API.AfterBefore.Before:
                        urlPath = $"/api/v5/tradingBot/grid/orders-algo-pending?algoOrdType=grid&before={algoId}";
                        break;
                    default:
                        throw new ArgumentException("Invalid direction.");
                }
            }
            else
            {
                throw new ArgumentException("Необходимо указать оба параметра: direction и algoId.");
            }

            return API.Get_Async(urlPath);
        }


    }
}
