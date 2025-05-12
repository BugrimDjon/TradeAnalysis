using bot_analysis.API;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_analysis.SQL
{
    internal class InstId
    {
        public static async Task UpdateTradingPairsFromBots(string connectionString)
        {
            // Запрос: вставить уникальные InstId из gridbots в таблицу InstId, если их там ещё нет
            string query = @"
                INSERT INTO InstId (OKX)
                SELECT DISTINCT InstId
                FROM gridbots
                WHERE InstId IS NOT NULL
                AND InstId NOT IN (SELECT OKX FROM InstId);";

            // Открытие подключения
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            // Выполнение запроса
            using var command = new MySqlCommand(query, connection);
            await command.ExecuteNonQueryAsync(); // ← важно!
        }
    }
}