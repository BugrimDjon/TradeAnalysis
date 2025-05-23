
//using bot_analysis.SQL;
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

                Server = Environment.GetEnvironmentVariable("DB_SERVER")??
                    throw new InvalidOperationException("DB_SERVER настройте подключение к БД");
                Database = Environment.GetEnvironmentVariable("DB_DATABASE")??
                    throw new InvalidOperationException("DB_DATABASE настройте подключение к БД");
                User = Environment.GetEnvironmentVariable("DB_USER")??
                    throw new InvalidOperationException("DB_USER настройте подключение к БД");
                Password = Environment.GetEnvironmentVariable("DB_PASSWORD")??
                    throw new InvalidOperationException("DB_PASSWORD настройте подключение к БД");
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

                ApiKey = Environment.GetEnvironmentVariable("DB_APIKEY")??
                    throw new InvalidOperationException("DB_APIKEY настройте подключение к API");
                SecretKey = Environment.GetEnvironmentVariable("DB_SECRETKEY")??
                    throw new InvalidOperationException("DB_SECRETKEY настройте подключение к API");
                Passphrase = Environment.GetEnvironmentVariable("DB_PASSPHRASE")??
                    throw new InvalidOperationException("DB_PASSPHRASE настройте подключение к API");
            }
        }
    }
}
