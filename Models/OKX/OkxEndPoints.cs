using bot_analysis.Config.OKX;
using System.Security.Policy;
namespace bot_analysis.Models.OKX
{
    public class OkxEndpointInfo
    {
        public string Url { get; set; }
        public double Frequency { get; set; } // частота запросов, например, в запросах в секунду
    }


    public static class OkxEndPoints
    {
        //Для вычитки баланса аккаунта
        public static OkxEndpointInfo BalansAcaunt => new OkxEndpointInfo
        {
            Url = $"/api/v5/account/balance?limit={AppOkxDataApi.LimitEndPoint}",
            Frequency = 5
        };

        //для запроса bills
        public static OkxEndpointInfo Bill => new OkxEndpointInfo
        {
            Url = $"/api/v5/account/bills-archive?limit={AppOkxDataApi.LimitEndPoint}",
            Frequency = 2.2
        };


        //Для запроса ордеров ботов
        public static OkxEndpointInfo SubOrdersBot(string algoId) => new OkxEndpointInfo
        {
            Url = $"/api/v5/tradingBot/grid/sub-orders?limit={AppOkxDataApi.LimitEndPoint}&algoOrdType=grid&type=filled&algoId={algoId}",
            Frequency = 10
        };

        //Для запроса ботов закончивших работу
        public static OkxEndpointInfo StoppedBot => new OkxEndpointInfo

        {
            Url = $"/api/v5/tradingBot/grid/orders-algo-history?algoOrdType=grid&limit={AppOkxDataApi.LimitEndPoint}",
            Frequency = 10
        };

        //Для запроса работающих ботов
        public static OkxEndpointInfo RunningBot => new OkxEndpointInfo
        {
            Url = $"/api/v5/tradingBot/grid/orders-algo-pending?algoOrdType=grid&limit={AppOkxDataApi.LimitEndPoint}",
            Frequency = 10
        };
        //Для запроса ручных сделок на споте
        public static OkxEndpointInfo FillHistorySpot => new OkxEndpointInfo
        {
            Url = $"/api/v5/trade/fills-history?instType=SPOT&limit={AppOkxDataApi.LimitEndPoint}",
            Frequency = 5
        };
        public static OkxEndpointInfo Candles(string instId, string bar) => new OkxEndpointInfo
        {
            Url = $"/api/v5/market/candles?&limit={AppOkxDataApi.LimitEndPoint}&bar={bar}&instId={instId}",
            Frequency = 20
        };
    }
}
