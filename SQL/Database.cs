using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient; // Добавить обязательно
using bot_analysis.Models;
using Mysqlx.Crud;

namespace bot_analysis.SQL
{
    public class Database
    {
        public static async Task<bool> SearchByFieldAsync(string tableName, string fieldName, string value, string connectionString)
        {
            string query = $"SELECT COUNT(*) FROM `{tableName}` WHERE `{fieldName}` = @value";

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@value", value);

            var result = await command.ExecuteScalarAsync();
            int count = Convert.ToInt32(result);

            return count > 0;
        }

        public static async Task InsertGridBotAsync(GridBotData bot, string connectionString)
        {
            string query = @"
        INSERT INTO gridbots (
            AlgoId, AlgoOrdType, InstId, InstType, State, Investment, BaseSz, QuoteSz, GridNum, GridProfit,
            FloatProfit, TotalPnl, PnlRatio, CTime, UTime, StopType, StopResult, CancelType,
            SlTriggerPx, TpTriggerPx, MaxPx, MinPx)
        VALUES (
            @AlgoId, @AlgoOrdType, @InstId, @InstType, @State, @Investment, @BaseSz, @QuoteSz, @GridNum, @GridProfit,
            @FloatProfit, @TotalPnl, @PnlRatio, @CTime, @UTime, @StopType, @StopResult, @CancelType,
            @SlTriggerPx, @TpTriggerPx, @MaxPx, @MinPx)
        ON DUPLICATE KEY UPDATE
            AlgoOrdType = VALUES(AlgoOrdType),
            InstId = VALUES(InstId),
            InstType = VALUES(InstType),
            State = VALUES(State),
            Investment = VALUES(Investment),
            BaseSz = VALUES(BaseSz),
            QuoteSz = VALUES(QuoteSz),
            GridNum = VALUES(GridNum),
            GridProfit = VALUES(GridProfit),
            FloatProfit = VALUES(FloatProfit),
            TotalPnl = VALUES(TotalPnl),
            PnlRatio = VALUES(PnlRatio),
            CTime = VALUES(CTime),
            UTime = VALUES(UTime),
            StopType = VALUES(StopType),
            StopResult = VALUES(StopResult),
            CancelType = VALUES(CancelType),
            SlTriggerPx = VALUES(SlTriggerPx),
            TpTriggerPx = VALUES(TpTriggerPx),
            MaxPx = VALUES(MaxPx),
            MinPx = VALUES(MinPx);
    ";


            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@AlgoId", bot.AlgoId);
            command.Parameters.AddWithValue("@AlgoOrdType", bot.AlgoOrdType);
            command.Parameters.AddWithValue("@InstId", bot.InstId);
            command.Parameters.AddWithValue("@InstType", bot.InstType);
            command.Parameters.AddWithValue("@State", bot.State);
            command.Parameters.AddWithValue("@Investment", bot.Investment);
            command.Parameters.AddWithValue("@BaseSz", bot.BaseSz);
            command.Parameters.AddWithValue("@QuoteSz", bot.QuoteSz);
            command.Parameters.AddWithValue("@GridNum", bot.GridNum);
            command.Parameters.AddWithValue("@GridProfit", bot.GridProfit);
            command.Parameters.AddWithValue("@FloatProfit", bot.FloatProfit);
            command.Parameters.AddWithValue("@TotalPnl", bot.TotalPnl);
            command.Parameters.AddWithValue("@PnlRatio", bot.PnlRatio);
            command.Parameters.AddWithValue("@CTime", bot.CTime);
            command.Parameters.AddWithValue("@UTime", bot.UTime);
            command.Parameters.AddWithValue("@StopType", bot.StopType);
            command.Parameters.AddWithValue("@StopResult", bot.StopResult);
            command.Parameters.AddWithValue("@CancelType", bot.CancelType);
            command.Parameters.AddWithValue("@SlTriggerPx",
                decimal.TryParse(bot.SlTriggerPx, out var sl) ? sl : (object)DBNull.Value);
            command.Parameters.AddWithValue("@TpTriggerPx",
                decimal.TryParse(bot.TpTriggerPx, out var tp) ? tp : (object)DBNull.Value);
            command.Parameters.AddWithValue("@MaxPx",
                decimal.TryParse(bot.MaxPx, out var max) ? max : (object)DBNull.Value);
            command.Parameters.AddWithValue("@MinPx",
                decimal.TryParse(bot.MinPx, out var min) ? min : (object)DBNull.Value);
            await command.ExecuteNonQueryAsync();
        }




        
        public static async Task<string> GetLastOrdIdFromDbAsync(string algoId, string connectionString)
        {
            //string query = "SELECT ordId FROM bot_orders WHERE algoId = @algoId ORDER BY cTime DESC LIMIT 1";
            //string query = "SELECT algoId FROM bot_orders ORDER BY CAST(cTime AS UNSIGNED) DESC LIMIT 1";
            string query = "SELECT ordId FROM bot_orders WHERE algoId = @algoId ORDER BY CAST(cTime AS UNSIGNED) DESC LIMIT 1";



            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@algoId", algoId);

            var result = await command.ExecuteScalarAsync();
            return result?.ToString();
        }

        public static async Task<string> GetValueFromDbAsync(string connectionString, string query, Dictionary<string, object> parameters = null)
        {
            //string query = "SELECT ordId FROM bot_orders WHERE algoId = @algoId ORDER BY cTime DESC LIMIT 1";
            //string query = "SELECT algoId FROM bot_orders ORDER BY CAST(cTime AS UNSIGNED) DESC LIMIT 1";
            //string query = "SELECT ordId FROM bot_orders WHERE algoId = @algoId ORDER BY CAST(cTime AS UNSIGNED) DESC LIMIT 1";

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand(query, connection);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
            }

            var result = await command.ExecuteScalarAsync();
            return result?.ToString();
        }









        public static async Task InsertOrderAsync(bot_orders order, string connectionString)
        {
            string query = @"
            INSERT INTO bot_orders 
            (algoId, algoClOrdId, algoOrdType, instType, instId, groupId, ordId, cTime, uTime, tdMode, 
                ccy, ordType, sz, state, side, px, avgPx, accFillSz, fee, feeCcy, rebate, rebateCcy, pnl, 
                posSide, lever, ctVal, tag)
            VALUES 
            (@algoId, @algoClOrdId, @algoOrdType, @instType, @instId, @groupId, @ordId, @cTime, @uTime, @tdMode, 
                @ccy, @ordType, @sz, @state, @side, @px, @avgPx, @accFillSz, @fee, @feeCcy, @rebate, @rebateCcy, 
                @pnl, @posSide, @lever, @ctVal, @tag)
            ON DUPLICATE KEY UPDATE 
                algoId=VALUES(algoId),
                algoClOrdId=VALUES(algoClOrdId),
                algoOrdType=VALUES(algoOrdType),
                instType=VALUES(instType),
                instId=VALUES(instId),
                groupId=VALUES(groupId),
                ordId=VALUES(ordId),
                cTime=VALUES(cTime),
                uTime=VALUES(uTime),
                tdMode=VALUES(tdMode),
                ccy=VALUES(ccy),
                ordType=VALUES(ordType),
                sz=VALUES(sz),
                state=VALUES(state),
                side=VALUES(side),
                px=VALUES(px),
                avgPx=VALUES(avgPx),
                accFillSz=VALUES(accFillSz),
                fee=VALUES(fee),
                feeCcy=VALUES(feeCcy),
                rebate=VALUES(rebate),
                rebateCcy=VALUES(rebateCcy),
                pnl=VALUES(pnl),
                posSide=VALUES(posSide),
                lever=VALUES(lever),
                ctVal=VALUES(ctVal),
                tag=VALUES(tag);";


            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand(query, connection);

            // Привязка всех параметров
            command.Parameters.AddWithValue("@algoId", order.algoId);
            command.Parameters.AddWithValue("@algoClOrdId", order.algoClOrdId);
            command.Parameters.AddWithValue("@algoOrdType", order.algoOrdType);
            command.Parameters.AddWithValue("@instType", order.instType);
            command.Parameters.AddWithValue("@instId", order.instId);
            command.Parameters.AddWithValue("@groupId", order.groupId);
            command.Parameters.AddWithValue("@ordId", order.ordId);
            command.Parameters.AddWithValue("@cTime", order.cTime);
            command.Parameters.AddWithValue("@uTime", order.uTime);
            command.Parameters.AddWithValue("@tdMode", order.tdMode);
            command.Parameters.AddWithValue("@ccy", order.ccy);
            command.Parameters.AddWithValue("@ordType", order.ordType);
            command.Parameters.AddWithValue("@sz", order.sz);
            command.Parameters.AddWithValue("@state", order.state);
            command.Parameters.AddWithValue("@side", order.side);
            command.Parameters.AddWithValue("@px", order.px);
            command.Parameters.AddWithValue("@avgPx", order.avgPx);
            command.Parameters.AddWithValue("@accFillSz", order.accFillSz);
            command.Parameters.AddWithValue("@fee", order.fee);
            command.Parameters.AddWithValue("@feeCcy", order.feeCcy);
            command.Parameters.AddWithValue("@rebate", order.rebate);
            command.Parameters.AddWithValue("@rebateCcy", order.rebateCcy);
            command.Parameters.AddWithValue("@pnl", order.pnl);
            command.Parameters.AddWithValue("@posSide", order.posSide);
            command.Parameters.AddWithValue("@lever", order.lever);
            command.Parameters.AddWithValue("@ctVal", order.ctVal);
            command.Parameters.AddWithValue("@tag", order.tag);

            await command.ExecuteNonQueryAsync();
        }



    }

}
