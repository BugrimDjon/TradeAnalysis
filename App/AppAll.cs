using bot_analysis.SQL;
using DotNetEnv;
using System;

namespace bot_analysis.App
{
    public static class AppAll
    {
        public static class AppSql
        {
            public static readonly string Server;
            public static readonly string Database;
            public static readonly string User;
            public static readonly string Password;

            // Статический конструктор
            static AppSql()
            {
                Env.Load();
                //Console.WriteLine("Loaded DB_SERVER: " + Environment.GetEnvironmentVariable("DB_SERVER"));


                Server = Environment.GetEnvironmentVariable("DB_SERVER");
                Database = Environment.GetEnvironmentVariable("DB_DATABASE");
                User = Environment.GetEnvironmentVariable("DB_USER");
                Password = Environment.GetEnvironmentVariable("DB_PASSWORD");
            }

            public static string GetConnectionString()
            {
                return $"Server={Server};Database={Database};User={User};Password={Password};";
            }
        }

        public static class AppApiOKX
        {
            public static readonly string ApiKey;
            public static readonly string SecretKey;
            public static readonly string Passphrase;

            // Статический конструктор
            static AppApiOKX()
            {
                Env.Load();
                //Console.WriteLine("Loaded DB_SERVER: " + Environment.GetEnvironmentVariable("DB_SERVER"));


                ApiKey = Environment.GetEnvironmentVariable("DB_APIKEY");
                SecretKey = Environment.GetEnvironmentVariable("DB_SECRETKEY"); // исправлено
                Passphrase = Environment.GetEnvironmentVariable("DB_PASSPHRASE");
            }
        }
    }
}
