
using bot_analysis.Init.OKX;
using bot_analysis.Services.OKX;

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
            //Console.Clear();
            using CancellationTokenSource cts = new();
            // Запуск слежения за нажатием клавиш в отдельном потоке
            _ = Task.Run(() =>
            {
                Console.WriteLine("Нажмите Ctrl+Q для остановки.");

                while (true)
                {
                    var key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.Q && key.Modifiers.HasFlag(ConsoleModifiers.Control))
                    {
                        Console.WriteLine("Остановка по Ctrl+Q.");
                        cts.Cancel();
                        break;
                    }
                }
            });

            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    await RunOkx();
                    var time = await RateLimiter.EnforceRateLimit(1.0 / 30);
                    Console.WriteLine(time.ToString());
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Цикл остановлен.");
            }

            Console.WriteLine("Завершение работы.");

            //Console.WriteLine("Для выхода нажмите клавишу");
            //Console.Read();
        }
        static async Task RunOkx()
        {
            var (tradeAnalysisService, logger) = AppOkxInitializer.Initialize();

            logger.IsEnabled = true;
            logger.DebugEnabled = false;
            logger.InfoEnabled = true;

            // ---------await tradeAnalysisService.UpdateBalansAcauntAsync();


            //*******************Готово
            await tradeAnalysisService.UpdateBotsAsync();   //Обновление информации по ботам

            await tradeAnalysisService.UpdateTradesAsync();//Обновление ручных сделок 
            await tradeAnalysisService.UpdateAccountTransfersAsync();//Обновление переводов на счет

            await tradeAnalysisService.UpdateUniqueTradingPairsAsync();//обновить уникальные торговые пары
            await tradeAnalysisService.UpdateUniqueCoinsAsync();//обновить уникальные монеты
            await tradeAnalysisService.UpdateTransferEvaluationsAsync();//оценить "затраты" на переводы для
                                                                        //корректного вычисления средней цены покупки монеты

            //сгенерировать и созранить отчет
            await tradeAnalysisService.GenerateReportAsync(await tradeAnalysisService.GenerateReport());
        }
    }
}
