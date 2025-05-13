using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_analysis.Interfaces
{
    internal interface ITradeAnalysisService
    {

        Task AnalyzeTradesAsync(); //анализ ручных сделок 
    }

}
