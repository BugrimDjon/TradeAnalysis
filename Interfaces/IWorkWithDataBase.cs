using bot_analysis.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using bot_analysis.Models;

namespace bot_analysis.Interfaces
{
    internal interface IWorkWithDataBase
    {
        /// <summary>
        /// Сохранение ручных сделок в базу данных
        /// </summary>
        /// <param name="trades">Список ручных сделок</param>
        Task SavePageTradeFillsHistoryToDataBase(IEnumerable<TradeFillsHistory> trades);
        Task <string> SearcPointToReadNewDataForFillsHistory();
        Task SavePageAccountTransfersToDataBase(IEnumerable<Bill> trades);
        Task<string> SearchPointToReadNewDataForAccountTransfers();


        
    }
}
