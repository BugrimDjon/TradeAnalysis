using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bot_analysis.Enums;
using bot_analysis.Models;
//using bot_analysis.Response;

namespace bot_analysis.Interfaces
{ 
    public interface ITradeApiClient
    {
        Task<IEnumerable<OkxBot>> GetInfoBotsAsync(bool oldBot, PaginationDirection? afterBefore = null, string? billId = null); // Получение списка остановленных ботов
        //Task<ApiBotStatus> GetBotStatusAsync(string botId); // Получение сделок бота
        //Task<ApiBotStatus> GetBotStatusAsync(string botId); // Получение статуса конкретного бота
        Task<IEnumerable<OkxTradeFillsHistory>> GetTradesAsync(PaginationDirection? afterBefore = null,
                                                            string? billId = null); // Получение сделок
        //Task<IEnumerable<ApiOrder>> GetOpenOrdersAsync(); // Получение открытых ордеров
        Task<IEnumerable<OkxBill>> GetTransfersStateAsync
                (PaginationDirection? afterBefore = null, string? billId = null); // Получение переводов на субаккаут
        
        
        //получить Json по указанному пути urlPath
        Task<string> GetPageJsonAsync(string urlPath,
                                                   PaginationDirection? direction = null,
                                                   string? point = null);

        /// <summary>
        /// универсальный метод для запроса по API
        /// </summary>
        /// <returns> возвращает распарсеный JSON в виде списка List<TData></returns>
        Task<IEnumerable<TData>> GetApiDataAsync<TResponse, TData>
                                        (string OkxUrl,
                                        PaginationDirection? afterBefore = null,
                                        string? pointRead = null)
             where TResponse : IApiResponseWithData<TData>;



    }

}
