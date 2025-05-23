
using bot_analysis.Init.OKX;

/*Получить активные боты	GET /api/v5/tradingBot/grid/orders-algo-pending
Получить завершённые (история) боты	GET /api/v5/tradingBot/grid/orders-algo-history
Получить ордера по конкретному боту	GET /api/v5/tradingBot/grid/sub-orders?algoId=...*/

namespace bot_analysis
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // очистить экран   
            Console.Clear();

            await RunOkx();

            Console.WriteLine("Для выхода нажмите клавишу");
            Console.Read();
        }
        static async Task RunOkx()
        {
            var (tradeAnalysisService, logger) = AppOkxInitializer.Initialize();

            logger.IsEnabled = true;

            await tradeAnalysisService.UpdateBotsAsync();   //Обновление информации по ботам

            await tradeAnalysisService.UpdateTradesAsync();//Обновление ручных сделок 
            await tradeAnalysisService.UpdateAccountTransfersAsync();//Обновление переводов на счет

            await tradeAnalysisService.UpdateUniqueTradingPairsAsync();//обновить уникальные торговые пары
            await tradeAnalysisService.UpdateUniqueCoinsAsync();//обновить уникальные монеты

            //сгенерировать и созранить отчет
            await tradeAnalysisService.GenerateReportAsync(await tradeAnalysisService.GenerateReport());
        }
    }
}
