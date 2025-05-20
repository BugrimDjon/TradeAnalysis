using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_analysis.Models
{
    public static class OkxUrlConst
    {
        //для запроса bills
        public const string Bills = "/api/v5/account/bills-archive?limit=100";

        //Для запроса ордеров ботов
        public static string SubOrdersBot(string algoId)=> 
            $"/api/v5/tradingBot/grid/sub-orders?limit=100&algoOrdType=grid&type=filled&algoId={algoId}";
        // частота заппросов SubOrdersBot
        public static int RequestFrequencySubOrdersBot = 10;


    }
}
