using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace bot_analysis.Interfaces
{
    internal interface ITradeAnalysisService
    {

        Task UpdateTradesAsync(); //Обновление ручных сделок 
        Task UpdateAccountTransfers();//Обновить переводы аккаунта
    }

}
