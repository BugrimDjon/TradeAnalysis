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
        void Debug(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message, Exception? ex = null);
    }

}
