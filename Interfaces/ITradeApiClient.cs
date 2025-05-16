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
        //Task<IEnumerable<ApiBot>> GetAllBotsAsync(); // Получение списка всех ботов
        //Task<ApiBotStatus> GetBotStatusAsync(string botId); // Получение сделок бота
        //Task<ApiBotStatus> GetBotStatusAsync(string botId); // Получение статуса конкретного бота
        Task<IEnumerable<TradeFillsHistory>> GetTradesAsync(PaginationDirection? afterBefore = null,
                                                            string? billId = null); // Получение сделок
        //Task<IEnumerable<ApiOrder>> GetOpenOrdersAsync(); // Получение открытых ордеров
        Task<IEnumerable<Bill>> GetTransfersStateAsync
                (PaginationDirection? afterBefore = null, string? billId = null); // Получение переводов на субаккаут
    }

}
