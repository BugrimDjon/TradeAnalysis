namespace bot_analysis.Services.OKX
{

    public static class RateLimiter
    {
        private static DateTime _lastRequestTime = DateTime.MinValue;

        public static async Task<TimeSpan?> EnforceRateLimit(double maxRequestsPerSecond)
        {
            //если входной параметр меньше или равен нулю, значить паузу не делаем
            if (maxRequestsPerSecond <= 0)
                return TimeSpan.Zero;

            //определение нужной паузы по входному параменту
            TimeSpan minInterval = TimeSpan.FromSeconds(1 / maxRequestsPerSecond);

            //фиксация текущего времени
            DateTime now = DateTime.UtcNow;

            //определение разницы между текущим временем и последним обращением 
            TimeSpan elapsed = now - _lastRequestTime;

            //если после последнего вызова и необходимой паузой прошло меньше времени
            TimeSpan waitTime= TimeSpan.Zero;
            if (elapsed < minInterval)
            {
                //сделать паузу
                waitTime = minInterval - elapsed;
                //Console.WriteLine("Пауза на " + waitTime + " с");
                await Task.Delay(waitTime);
            }

            //завиксировать последнне время вызова этого метода
            _lastRequestTime = DateTime.UtcNow;
            return waitTime;
        }
    }
}
