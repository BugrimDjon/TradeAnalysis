using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_analysis.Models.OKX
{
    public static class OkxUrlConst
    {
        //для запроса bills
        public const string Bill = "/api/v5/account/bills-archive?limit=100";

        //Для запроса ордеров ботов
        public static string SubOrdersBot(string algoId) =>
            $"/api/v5/tradingBot/grid/sub-orders?limit=100&algoOrdType=grid&type=filled&algoId={algoId}";

        //Для запроса ботов закончивших работу
        public const string StoppedBot =
            "/api/v5/tradingBot/grid/orders-algo-history?algoOrdType=grid&limit=100";

        //Для запроса работающих ботов
        public const string RunningBot =
            "/api/v5/tradingBot/grid/orders-algo-pending?algoOrdType=grid&limit=100";

        //Для запроса ручных сделок на споте
        public const string FillHistorySpot =
            "/api/v5/trade/fills-history?instType=SPOT&limit=100";
    }
}
