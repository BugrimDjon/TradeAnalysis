using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bot_analysis.SQL;
using MySqlX.XDevAPI.Common;
using Mysqlx.Session;

namespace bot_analysis.TotalResults
{
    public class Results
    {
        public static async Task<List<CoinStats>> GetCoinStatsGroupedByInstIdAsync(string connectionString)
        {
            var result = new List<CoinStats>();

            var query = @"
        SELECT 
            g.InstId,
            SUM(CASE WHEN g.State = 'stopped' THEN o.coin_delta ELSE 0 END) AS coin_stopped,
            SUM(CASE WHEN g.State = 'stopped' THEN o.usdt_delta ELSE 0 END) AS usdt_stopped,
            SUM(CASE WHEN g.State <> 'stopped' THEN o.coin_delta ELSE 0 END) AS coin_running,
            SUM(CASE WHEN g.State <> 'stopped' THEN o.usdt_delta ELSE 0 END) AS usdt_running
        FROM 
            gridbots g
        JOIN 
            bot_orders o ON o.algoId = g.AlgoId
        GROUP BY 
            g.InstId
        ORDER BY 
            g.InstId;";

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new CoinStats
                {
                    InstId = reader["InstId"].ToString(),
                    CoinStopped = Convert.ToDecimal(reader["coin_stopped"]),
                    UsdtStopped = Convert.ToDecimal(reader["usdt_stopped"]),
                    CoinRunning = Convert.ToDecimal(reader["coin_running"]),
                    UsdtRunning = Convert.ToDecimal(reader["usdt_running"]),
                });
            }

            return result;
        }

        public static async Task PrintResultBot(List<CoinStats> result)
        {
            // Заголовок таблицы
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(new string('-', 87)); // линия-разделитель
            Console.WriteLine("{0,-15} | {1,15} | {2,15} | {3,15} | {4,15}",
                "Njhujdfz ", "CoinStopped", "UsdtStopped", "CoinRunning", "UsdtRunning");
            Console.WriteLine(new string('-', 87)); // линия-разделитель

            // Строки таблицы
            foreach (var stat in result)
            {
                Console.WriteLine("{0,-15} | {1,15:F4} | {2,15:F2} | {3,15:F4} | {4,15:F2}",
                    stat.InstId,
                    stat.CoinStopped,
                    stat.UsdtStopped,
                    stat.CoinRunning,
                    stat.UsdtRunning);
            }
        }
        public static async Task PrintResultBotTest()
        {
            var result = new List<CoinStats> ();
            result.Add(new CoinStats
            {
                InstId = "FET - USDT",
                CoinStopped =  Convert.ToDecimal(-52.6469),
                UsdtStopped = Convert.ToDecimal(47.44),
                CoinRunning = Convert.ToDecimal(31.8585),
                UsdtRunning = Convert.ToDecimal(-25.16)
            });

            result.Add(new CoinStats
            {
                InstId = "LSK-USDT",
                CoinStopped = Convert.ToDecimal(0),
                UsdtStopped = Convert.ToDecimal(0),
                CoinRunning = Convert.ToDecimal(-14.7924),
                UsdtRunning = Convert.ToDecimal(7.86)
            });


            
            //  Строка 1
            Console.WriteLine(new string('-', 120)); // линия-разделитель
            Console.WriteLine("{0,7}{1,-8} | {2,40} {3,-45}", "","Боты", "","Торговые пары");
            Console.Write("{0,-15} |", "");
            Console.WriteLine(new string('-', 103)); // линия-разделитель
            //  Строка 2
            Console.Write("{0,-15} |", "");
            foreach (var temp in result)
            {
                Console.Write("{0,15}{1,5} |",temp.InstId,"");
            }
            Console.WriteLine();
            Console.WriteLine(new string('-', 120)); // линия-разделитель

            //  Строка 3
            Console.Write("{0,-15} |", "Остановленные");
            foreach (var temp in result)
            {
                Console.Write("{0,9} |",temp.CoinStopped);
                Console.Write("{0,9} |",temp.UsdtStopped);
            }
            Console.WriteLine();



            
            //  Строка 4
            Console.WriteLine(new string('-', 120)); // линия-разделитель
            Console.Write("{0,-15} |", "Работающие");
            foreach (var temp in result)
            {
                Console.Write("{0,9} |", temp.CoinRunning);
                Console.Write("{0,9} |", temp.UsdtRunning);
            }
            Console.WriteLine();
            Console.WriteLine(new string('~', 120)); // линия-разделитель
            


            decimal sumUsdt = 0;
            //  Строка 5
            Console.Write("{0,-15} |", "Итого");
            foreach (var temp in result)
            {
                Console.Write("{0,9} |", (temp.CoinRunning+ temp.CoinStopped));
                Console.Write("{0,9} |", (temp.UsdtRunning+ temp.UsdtStopped));

                sumUsdt += temp.UsdtRunning + temp.UsdtStopped;

            }
            Console.WriteLine();
            Console.WriteLine(new string('-', 120)); // линия-разделитель

            //  Строка 6
            Console.Write("{0,-15} |", "Итого в USDT");
                
                Console.Write("{0,20}", sumUsdt);
            Console.WriteLine();
            Console.WriteLine(new string('-', 120)); // линия-разделитель

        }


        //public static void PrintResultBotGpt(List<CoinStats> result)
        public static async Task PrintResultBotGpt(List<CoinStats> result)
        {
            /*var result = new List<CoinStats>();
            result.Add(new CoinStats
            {
                InstId = "FET - USDT",
                CoinStopped = Convert.ToDecimal(-52.6469),
                UsdtStopped = Convert.ToDecimal(47.44),
                CoinRunning = Convert.ToDecimal(31.8585),
                UsdtRunning = Convert.ToDecimal(-25.16)
            });

            result.Add(new CoinStats
            {
                InstId = "LSK-USDT",
                CoinStopped = Convert.ToDecimal(0),
                UsdtStopped = Convert.ToDecimal(0),
                CoinRunning = Convert.ToDecimal(-14.7924),
                UsdtRunning = Convert.ToDecimal(7.86)
            });
*/



            if (result == null || result.Count == 0)
            {
                Console.WriteLine("Нет данных для отображения.");
                return;
            }

            const int columnWidth = 20;
            int tableWidth = result.Count * columnWidth * 2 + 20;

            decimal totalUsdt = 0;

            // Линия-разделитель
            void PrintSeparator(char ch = '-',int subtractFromLength=0) => 
                Console.WriteLine(new string(ch, tableWidth- subtractFromLength));

            // Печать заголовка
            PrintSeparator();
            Console.WriteLine("{0,7}{1,-8} | {2,40} {3,-40}", "", "БОТЫ", "ТОРГОВЫЕ ПАРЫ","");
            Console.Write("{0,-15} |", "");
            PrintSeparator('-',17);
            Console.Write("{0,-15} |", "");
            
            foreach (var coin in result)
                Console.Write($"{coin.InstId,15}      |");
            Console.WriteLine();
            PrintSeparator();

            // Строка: Остановленные
            Console.Write("{0,-15} |", "Остановленные");
            foreach (var coin in result)
            {
                Console.Write($"{coin.CoinStopped,9:F4} |");
                Console.Write($"{coin.UsdtStopped,9:F2} |");
            }
            Console.WriteLine();

            // Строка: Работающие
            PrintSeparator();
            Console.Write("{0,-15} |", "Работающие");
            foreach (var coin in result)
            {
                Console.Write($"{coin.CoinRunning,9:F4} |");
                Console.Write($"{coin.UsdtRunning,9:F2} |");
            }
            Console.WriteLine();

            // Строка: Итого
            PrintSeparator('~');
            Console.Write("{0,-15} |", "Итого");
            foreach (var coin in result)
            {
                var totalCoin = coin.CoinRunning + coin.CoinStopped;
                var totalCoinUsdt = coin.UsdtRunning + coin.UsdtStopped;

                Console.Write($"{totalCoin,9:F4} |");
                Console.Write($"{totalCoinUsdt,9:F2} |");

                totalUsdt += totalCoinUsdt;
            }
            Console.WriteLine();

            // Строка: Общая сумма в USDT
            PrintSeparator();
            Console.Write("{0,-15} |", "Итого в USDT");
            Console.Write($"{totalUsdt,20:F2}");
            Console.WriteLine();
            PrintSeparator();
        }



    }








    public class CoinStats
    {
        public string InstId { get; set; }
        public decimal CoinStopped { get; set; }
        public decimal UsdtStopped { get; set; }
        public decimal CoinRunning { get; set; }
        public decimal UsdtRunning { get; set; }
    }

}
