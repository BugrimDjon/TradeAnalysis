using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_analysis.Services
{
    
    public static class RateLimiter
    {
        private static DateTime _lastRequestTime = DateTime.MinValue;

        public static async Task EnforceRateLimit(double maxRequestsPerSecond)
        {
            //если входной параметр меньше или равен нулю, значить паузу не делаем
            if (maxRequestsPerSecond <= 0)
                return;

            //определение нужной паузы по входному параменту
            TimeSpan minInterval = TimeSpan.FromSeconds(1 / maxRequestsPerSecond);

            //фиксация текущего времени
            DateTime now = DateTime.UtcNow;

            //определение разницы между текущим временем и последним обращением 
            TimeSpan elapsed = now - _lastRequestTime;

            //если после последнего вызова и необходимой паузой прошло меньше времени
            if (elapsed < minInterval)
            {
                //сделать паузу
                TimeSpan waitTime = minInterval - elapsed;
                Console.WriteLine("Пауза на " + waitTime + " с");
                await Task.Delay(waitTime);
            }

            //завиксировать последнне время вызова этого метода
            _lastRequestTime = DateTime.UtcNow;

        }

    }
}
