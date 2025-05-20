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
        Task SavePageTradeFillsHistoryToDataBase(IEnumerable<OkxTradeFillsHistory> trades);
        Task <string> SearcPointToReadNewDataForFillsHistory();
        Task SavePageStoppedBotToDataBase(IEnumerable<OkxBot> bots); // сохранить данные про остановленных ботов
        Task SavePageAccountTransfersToDataBase(IEnumerable<OkxBill> trades);
        Task<string> SearchPointToReadNewDataForAccountTransfers();
        Task<string> SearchPointToReadNewDataForStoppedBot();
        Task UpdateUniqueTradingPairsInBD();//обновить используемые торговые пары
        Task UpdateUniqueCoinsInBD();//обновить используемые монеты
        Task <IEnumerable<string>> GetUniqueCoinsAsync();//получить перечень уникальных монет
        Task ExecuteSQLQueryWithoutReturningParameters(string query); // выполняет переданный запрос
        Task<string> ExecuteSqlQueryReturnParamString(string query);// возвращает string на переданный запрос

        Task SaveOrdStoppedBotsToDB(IEnumerable<OkxBotOrder> BotOrder); //сохраняет данные о сделках ботов

    }
}
