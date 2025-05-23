using DotNetEnv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_analysis.Config.OKX
{
    public static class AppDataApiOKX
    {
        public static readonly string ApiKey;
        public static readonly string SecretKey;
        public static readonly string Passphrase;

        // Статический конструктор
        static AppDataApiOKX()
        {
            Env.Load();
            //Console.WriteLine("Loaded DB_SERVER: " + Environment.GetEnvironmentVariable("DB_SERVER"));

            ApiKey = Environment.GetEnvironmentVariable("DB_APIKEY");
            SecretKey = Environment.GetEnvironmentVariable("DB_SECRETKEY"); // исправлено
            Passphrase = Environment.GetEnvironmentVariable("DB_PASSPHRASE");
        }
    }
}
