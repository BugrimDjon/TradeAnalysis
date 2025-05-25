using bot_analysis.Models.OKX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace bot_analysis.Interfaces
{
    public interface ITradeAnalysisService
    {
        Task UpdateBalansAcauntAsync(); //Обновление баланса по акаунту
        Task UpdateBotsAsync();//Обновление информации по ботам
        Task UpdateTradesAsync(); //Обновление ручных сделок 
        Task UpdateAccountTransfersAsync();//Обновить переводы аккаунта
        Task UpdateUniqueTradingPairsAsync();//Обновить уникальные торговые пары
        Task UpdateUniqueCoinsAsync();//Обновить уникальные торговые пары
        Task<IEnumerable<OkxReport>> GenerateReport();//Сформировать отчет
        Task GenerateReportAsync(IEnumerable<OkxReport> data);//Сохранить отчет
        Task UpdateTransferEvaluationsAsync(); //обновить затраченные средства для переводов на/c аккаунта
    }
}
