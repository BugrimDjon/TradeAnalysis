using bot_analysis.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_analysis.Config
{
    public class ConsoleLogger : ILogger
    {
        // Включён ли лог
        public bool IsEnabled { get; set; } = true;
        public bool DebugEnabled { get; set; } = true;
        public bool InfoEnabled { get; set; } = true;
        public bool WarningEnabled { get; set; } = true;
        public bool ErrorEnabled { get; set; } = true;




        public void Debug(string message)
        {
            if (IsEnabled && DebugEnabled)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"[DEBUG] {DateTime.Now:u} — {message}");
                Console.ResetColor();
            }
        }

        public void Info(string message)
        {
            if (IsEnabled && InfoEnabled)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"[INFO ] {DateTime.Now:u} — {message}");
                Console.ResetColor();
            }
        }

        public void Warning(string message)
        {
            if (IsEnabled && WarningEnabled)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[WARN ] {DateTime.Now:u} — {message}");
                Console.ResetColor();
            }
        }

        public void Error(string message, Exception? ex = null)
        {
            if (IsEnabled && ErrorEnabled)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] {DateTime.Now:u} — {message}");
                if (ex != null)
                {
                    Console.WriteLine(ex.ToString());
                }
                Console.ResetColor();
            }
        }
    }
}
