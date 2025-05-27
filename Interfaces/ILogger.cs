using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_analysis.Interfaces
{
    public interface ILogger
    {
        bool IsEnabled { get; set; }
        public bool DebugEnabled { get; set; }
        public bool InfoEnabled { get; set; }
        public bool WarningEnabled { get; set; }
        public bool ErrorEnabled { get; set; }
        void Debug(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message, Exception? ex = null);
    }
}
