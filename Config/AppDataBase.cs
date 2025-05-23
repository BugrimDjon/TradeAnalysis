using DotNetEnv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_analysis.Config
{
    internal class AppDataBase
    {
        public static readonly string Server;
        public static readonly string Database;
        public static readonly string User;
        public static readonly string Password;

        // Статический конструктор
        static AppDataBase()
        {
            Env.Load();
            //Console.WriteLine("Loaded DB_SERVER: " + Environment.GetEnvironmentVariable("DB_SERVER"));

            Server = Environment.GetEnvironmentVariable("DB_SERVER")??
                throw new InvalidOperationException("DB_SERVER настройте подключение к БД");
            Database = Environment.GetEnvironmentVariable("DB_DATABASE")??
                throw new InvalidOperationException("DB_DATABASE настройте подключение к БД");
            User = Environment.GetEnvironmentVariable("DB_USER")??
                throw new InvalidOperationException("DB_USER настройте подключение к БД");            Password = Environment.GetEnvironmentVariable("DB_PASSWORD")??
                throw new InvalidOperationException("DB_PASSWORD настройте подключение к БД");
        }

        public static string ConnectionStringForDB()
        {
            return $"Server={Server};Database={Database};User={User};Password={Password};";
        }
    }
}
