using bot_analysis.Models;
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
    internal interface ITradeAnalysisService
    {

        Task UpdateBotsAsync();//Обновление информации по ботам
        Task UpdateTradesAsync(); //Обновление ручных сделок 
        Task UpdateAccountTransfersAsync();//Обновить переводы аккаунта
        Task UpdateUniqueTradingPairsAsync();//Обновить уникальные торговые пары
        Task UpdateUniqueCoinsAsync();//Обновить уникальные торговые пары
        Task<IEnumerable<OkxReport>> GenerateReport();//Сформировать отчет
        Task GenerateReportAsync(IEnumerable<OkxReport> data);//Сохранить отчет
        
    }

}
