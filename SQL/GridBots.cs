using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bot_analysis.App;
using MySql.Data.MySqlClient;

namespace bot_analysis.SQL
{
    internal class GridBots
    {
        /// <summary>
        /// Асинхронно получает список AlgoId всех ботов со статусом "running" из таблицы gridbots.
        /// </summary>
        /// <returns>Список строк (AlgoId), где состояние бота — "running".</returns>
        public static async Task<List<string>> GetAlgoIdRunningAsync(string connectionString)
        {
            // Результирующий список для хранения AlgoId всех запущенных ботов
            var result = new List<string>();

            // Создаём подключение к базе данных с использованием строки подключения
            using var connection = new MySqlConnection(connectionString);
            // Асинхронно открываем подключение
            await connection.OpenAsync();
            // SQL-запрос для выбора всех AlgoId из таблицы, где состояние равно 'running'
            var query = "SELECT AlgoId FROM gridbots WHERE State = 'running'";

            using var command = new MySqlCommand(query, connection);
            var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                // Читаем значение первого (и единственного) столбца — AlgoId — и добавляем в список
                result.Add(reader.GetString(0)); // 0 — индекс столбца AlgoId
            }
            // Возвращаем полученный список AlgoId
            return result;

            
        }

        public static async Task<string> GetStateByAlgoIdAsync(string AlgoId)
        {
            string query = $"SELECT State FROM gridbots " +
                           $"WHERE AlgoId = {AlgoId} ";


            using var connection = new MySqlConnection(AppAll.AppSql.GetConnectionString());
            await connection.OpenAsync();

            using var command = new MySqlCommand(query, connection);
            var result = await command.ExecuteScalarAsync();
            return result?.ToString();
        }


        public static async Task<string> GetAlgoIdStoppedAndIsProcessedAsync( string connectionString)
        {
            string query = "SELECT AlgoId FROM gridbots " +
                "WHERE IsProcessed = 'false' AND " +
                "State = 'stopped' ORDER BY CAST(AlgoId AS UNSIGNED) DESC LIMIT 1";
           
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand(query, connection);
            var result = await command.ExecuteScalarAsync();
            return result?.ToString();
        }

        public static async Task SetIsProcessedForAlgoId(string AlgoId,string connectionString)
        {
            string query = "UPDATE gridbots SET IsProcessed = true WHERE AlgoId ="+AlgoId;

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand(query, connection);
            await command.ExecuteNonQueryAsync();
        }
        public static async Task<bool> SearchAlgoIdAsinc(string algoId)
        {
            bool result = false;
            result = await Database.SearchByFieldAsync(
                                                    "gridbots",
                                                    "AlgoId",
                                                    algoId,
                                                    AppAll.AppSql.GetConnectionString());

            return (result);
        }
    }
}
