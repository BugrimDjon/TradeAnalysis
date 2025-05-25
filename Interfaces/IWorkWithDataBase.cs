using bot_analysis.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using bot_analysis.Models.OKX;
using System.Data;

namespace bot_analysis.Interfaces
{
    public interface IWorkWithDataBase
    {
        /// <summary>
        /// Сохранение ручных сделок в базу данных
        /// </summary>
        /// <param name="trades">Список ручных сделок</param>
        Task SavePageTradeFillsHistoryToDataBase(IEnumerable<OkxTradeFillsHistory> trades);
        Task SavePageBotToDataBase(IEnumerable<OkxBot> bots); // сохранить данные про остановленных ботов
        Task SavePageAccountTransfersToDataBase(IEnumerable<OkxBill> trades);
        Task UpdateUniqueTradingPairsInBD();//обновить используемые торговые пары
        Task UpdateUniqueCoinsInBD();//обновить используемые монеты
        Task <IEnumerable<string>> GetUniqueCoinsAsync();//получить перечень уникальных монет
        Task ExecuteSQLQueryWithoutReturningParameters(string query); // выполняет переданный запрос
        Task<string> ExecuteSqlQueryReturnParamString(string query);// возвращает string на переданный запрос
        Task<DataTable> ExecuteSqlQueryReturnDataTable(string query); //выполнить запрос и вернуть DataTable

        /// <summary>
        /// Принимает SQL запрос как параметр и возвращает ответ в виде списока
        /// </summary>
        /// <param name="query"> string SQL запрос</param>
        /// <returns> Task<IEnumerable<string>> ответ на запрос </returns>
        Task<IEnumerable<string>> ExecuteSqlQueryReturnParamListString(string query);
        Task SaveOrdStoppedBotsToDB(IEnumerable<OkxBotOrder> BotOrder); //сохраняет данные о сделках ботов
    }
}
