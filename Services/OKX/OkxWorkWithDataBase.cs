using bot_analysis.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Collections;
using Google.Protobuf.WellKnownTypes;
using System.Reflection.Metadata.Ecma335;
using Mysqlx.Crud;
using Mysqlx.Prepare;
using System.Diagnostics;
using System.Data.Common;
using bot_analysis.Models.OKX;
using System.Data;

namespace bot_analysis.Services.OKX
{
    internal class OkxWorkWithDataBase : IWorkWithDataBase
    {
        private readonly MySqlConnection _mySqlConnection;

        public OkxWorkWithDataBase(MySqlConnection mySqlConnection)
        {
            _mySqlConnection = mySqlConnection;
        }

        public async Task SavePageBotToDataBase(IEnumerable<OkxBot> bots)
        {
            const string query = @"
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
            MinPx = VALUES(MinPx);";

            if (_mySqlConnection.State != System.Data.ConnectionState.Open)
                await _mySqlConnection.OpenAsync();
            foreach (var trade in bots)
            {
                await using var cmd = new MySqlCommand(query, _mySqlConnection);

                cmd.Parameters.AddWithValue("@AlgoId", trade.AlgoId);
                cmd.Parameters.AddWithValue("@AlgoOrdType", trade.AlgoOrdType);
                cmd.Parameters.AddWithValue("@InstId", trade.InstId);
                cmd.Parameters.AddWithValue("@InstType", trade.InstType);
                cmd.Parameters.AddWithValue("@State", trade.State);
                cmd.Parameters.AddWithValue("@Investment", trade.Investment);
                cmd.Parameters.AddWithValue("@BaseSz", trade.BaseSz);
                cmd.Parameters.AddWithValue("@QuoteSz", trade.QuoteSz);
                cmd.Parameters.AddWithValue("@GridNum", trade.GridNum);
                cmd.Parameters.AddWithValue("@GridProfit", trade.GridProfit);
                cmd.Parameters.AddWithValue("@FloatProfit", trade.FloatProfit);
                cmd.Parameters.AddWithValue("@TotalPnl", trade.TotalPnl);
                cmd.Parameters.AddWithValue("@PnlRatio", trade.PnlRatio);
                cmd.Parameters.AddWithValue("@CTime", trade.CTime);
                cmd.Parameters.AddWithValue("@UTime", trade.UTime);
                cmd.Parameters.AddWithValue("@StopType", trade.StopType);
                cmd.Parameters.AddWithValue("@StopResult", trade.StopResult);
                cmd.Parameters.AddWithValue("@CancelType", trade.CancelType);
                cmd.Parameters.AddWithValue("@SlTriggerPx",
                    decimal.TryParse(trade.SlTriggerPx, out var sl) ? sl : DBNull.Value);
                cmd.Parameters.AddWithValue("@TpTriggerPx",
                    decimal.TryParse(trade.TpTriggerPx, out var tp) ? tp : DBNull.Value);
                cmd.Parameters.AddWithValue("@MaxPx",
                    decimal.TryParse(trade.MaxPx, out var max) ? max : DBNull.Value);
                cmd.Parameters.AddWithValue("@MinPx",
                    decimal.TryParse(trade.MinPx, out var min) ? min : DBNull.Value);
                await cmd.ExecuteNonQueryAsync();
            }

            await _mySqlConnection.CloseAsync();
        }

        public async Task SavePageAccountTransfersToDataBase(IEnumerable<OkxBill> trades)
        {
            const string query = @"
                INSERT INTO bills_table (
                bal, balChg, billId, ccy, clOrdId, execType, fee, 
                fillFwdPx, fillIdxPx, fillMarkPx, fillMarkVol, fillPxUsd, 
                fillPxVol, fillTime, `from`, instId, instType, interest, mgnMode, 
                notes, ordId, pnl, posBal, posBalChg, px, subType, sz, tag, `to`, tradeId, ts, `type`)
                values (
                    @bal, @balChg, @billId, @ccy, @clOrdId, @execType, @fee, 
                    @fillFwdPx, @fillIdxPx, @fillMarkPx, @fillMarkVol, @fillPxUsd, 
                    @fillPxVol, @fillTime, @from, @instId, @instType, @interest, @mgnMode, 
                    @notes, @ordId, @pnl, @posBal, @posBalChg, @px, @subType, @sz, @tag, @to, @tradeId, @ts, @type)

                ON DUPLICATE KEY UPDATE
                bal = VALUES(bal),
                balChg = VALUES(balChg),
                billId = VALUES(billId),
                ccy = VALUES(ccy),
                clOrdId = VALUES(clOrdId),
                execType = VALUES(execType),
                fee = VALUES(fee),
                fillFwdPx = VALUES(fillFwdPx),
                fillIdxPx = VALUES(fillIdxPx),
                fillMarkPx = VALUES(fillMarkPx),
                fillMarkVol = VALUES(fillMarkVol),
                fillPxUsd = VALUES(fillPxUsd),
                fillPxVol = VALUES(fillPxVol),
                fillTime = VALUES(fillTime),
                `from` = VALUES(`from`),
                instId = VALUES(instId),
                instType = VALUES(instType),
                interest = VALUES(interest),
                mgnMode = VALUES(mgnMode),
                notes = VALUES(notes),
                ordId = VALUES(ordId),
                pnl = VALUES(pnl),
                posBal = VALUES(posBal),
                posBalChg = VALUES(posBalChg),
                px = VALUES(px),
                subType = VALUES(subType),
                sz = VALUES(sz),
                tag = VALUES(tag),
                `to` = VALUES(`from`),
                tradeId = VALUES(tradeId),
                ts = VALUES(ts),
                `type` = VALUES(`type`);";

            if (_mySqlConnection.State != System.Data.ConnectionState.Open)
                await _mySqlConnection.OpenAsync();

            foreach (var trade in trades)
            {
                await using var cmd = new MySqlCommand(query, _mySqlConnection);
                cmd.Parameters.AddWithValue("@bal", ConvertToNullableDecimal(trade.Bal));
                cmd.Parameters.AddWithValue("@balChg", ConvertToNullableDecimal(trade.BalChg));
                cmd.Parameters.AddWithValue("@billId", trade.BillId);
                cmd.Parameters.AddWithValue("@ccy", trade.Ccy);
                cmd.Parameters.AddWithValue("@clOrdId", trade.ClOrdId);
                cmd.Parameters.AddWithValue("@execType", trade.ExecType);
                cmd.Parameters.AddWithValue("@fee", ConvertToNullableDecimal(trade.Fee));
                cmd.Parameters.AddWithValue("@fillFwdPx", ConvertToNullableDecimal(trade.FillFwdPx));
                cmd.Parameters.AddWithValue("@fillIdxPx", ConvertToNullableDecimal(trade.FillIdxPx));
                cmd.Parameters.AddWithValue("@fillMarkPx", ConvertToNullableDecimal(trade.FillMarkPx));
                cmd.Parameters.AddWithValue("@fillMarkVol", ConvertToNullableDecimal(trade.FillMarkVol));
                cmd.Parameters.AddWithValue("@fillPxUsd", ConvertToNullableDecimal(trade.FillPxUsd));
                cmd.Parameters.AddWithValue("@fillPxVol", ConvertToNullableDecimal(trade.FillPxVol));
                cmd.Parameters.AddWithValue("@fillTime", ConvertToNullableLong(trade.FillTime));
                cmd.Parameters.AddWithValue("@from", trade.From);
                cmd.Parameters.AddWithValue("@instId", trade.InstId);
                cmd.Parameters.AddWithValue("@instType", trade.InstType);
                cmd.Parameters.AddWithValue("@interest", ConvertToNullableDecimal(trade.Interest));
                cmd.Parameters.AddWithValue("@mgnMode", trade.MgnMode);
                cmd.Parameters.AddWithValue("@notes", trade.Notes);
                cmd.Parameters.AddWithValue("@ordId", ConvertToNullableLong(trade.OrdId));
                cmd.Parameters.AddWithValue("@pnl", ConvertToNullableDecimal(trade.Pnl));
                cmd.Parameters.AddWithValue("@posBal", ConvertToNullableDecimal(trade.PosBal));
                cmd.Parameters.AddWithValue("@posBalChg", ConvertToNullableDecimal(trade.PosBalChg));
                cmd.Parameters.AddWithValue("@px", ConvertToNullableDecimal(trade.Px));
                //Console.WriteLine("trade.SubType = " + trade.SubType);
                cmd.Parameters.AddWithValue("@subType", trade.SubType);

                cmd.Parameters.AddWithValue("@sz", ConvertToNullableDecimal(trade.Sz));
                cmd.Parameters.AddWithValue("@tag", trade.Tag);
                cmd.Parameters.AddWithValue("@to", trade.To);
                cmd.Parameters.AddWithValue("@tradeId", trade.TradeId);
                cmd.Parameters.AddWithValue("@ts", trade.Ts);

                //Console.WriteLine("trade.Type = " + trade.Type + "       ConvertToNullableLong(trade.Type) = " + ConvertToNullableLong(trade.Type));
                cmd.Parameters.AddWithValue("@type", ConvertToNullableLong(trade.Type));
                await cmd.ExecuteNonQueryAsync();
            }
            await _mySqlConnection.CloseAsync();
        }

        public async Task SaveOrdStoppedBotsToDB(IEnumerable<OkxBotOrder> BotOrder)
        {
            const string query = @"
    INSERT INTO bot_orders(algoId, algoClOrdId, algoOrdType, instType, instId, 
                groupId, ordId, cTime, uTime, tdMode, ccy, ordType, sz, state, 
                side, px, avgPx, accFillSz, fee, feeCcy, rebate, rebateCcy, pnl, 
                posSide, lever, ctVal, tag)

    VALUES (@algoId, @algoClOrdId, @algoOrdType, @instType, @instId,
                @groupId, @ordId, @cTime, @uTime, @tdMode, @ccy, @ordType, @sz, @state,
                @side, @px, @avgPx, @accFillSz, @fee, @feeCcy, @rebate, @rebateCcy, @pnl,
                @posSide, @lever, @ctVal, @tag)

    ON DUPLICATE KEY UPDATE
                algoId= VALUES( algoId), 
                algoClOrdId= VALUES( algoClOrdId), 
                algoOrdType= VALUES( algoOrdType), 
                instType= VALUES( instType), 
                instId= VALUES( instId), 
                groupId= VALUES( groupId), 
                cTime= VALUES( cTime), 
                uTime= VALUES( uTime), 
                tdMode= VALUES( tdMode), 
                ccy= VALUES( ccy), 
                ordType= VALUES( ordType), 
                sz= VALUES( sz), 
                state= VALUES( state), 
                side= VALUES( side), 
                px= VALUES( px), 
                avgPx= VALUES( avgPx), 
                accFillSz= VALUES( accFillSz), 
                fee= VALUES( fee), 
                feeCcy= VALUES( feeCcy), 
                rebate= VALUES( rebate), 
                rebateCcy= VALUES( rebateCcy), 
                pnl= VALUES( pnl), 
                posSide= VALUES( posSide), 
                lever= VALUES( lever), 
                ctVal= VALUES( ctVal), 
                tag= VALUES( tag)";

            if (_mySqlConnection.State != System.Data.ConnectionState.Open)
                await _mySqlConnection.OpenAsync();

            foreach (var trade in BotOrder)
            {
                await using var cmd = new MySqlCommand(query, _mySqlConnection);
                cmd.Parameters.AddWithValue("@algoId", ConvertToNullableLong(trade.algoId));
                cmd.Parameters.AddWithValue("@algoClOrdId", trade.algoClOrdId);
                cmd.Parameters.AddWithValue("@algoOrdType", trade.algoOrdType);
                cmd.Parameters.AddWithValue("@instType", trade.instType);
                cmd.Parameters.AddWithValue("@instId", trade.instId);
                cmd.Parameters.AddWithValue("@groupId", trade.groupId);
                cmd.Parameters.AddWithValue("@ordId", ConvertToNullableLong(trade.ordId));
                cmd.Parameters.AddWithValue("@cTime", ConvertToNullableLong(trade.cTime));
                cmd.Parameters.AddWithValue("@uTime", ConvertToNullableLong(trade.uTime));
                cmd.Parameters.AddWithValue("@tdMode", trade.tdMode);
                cmd.Parameters.AddWithValue("@ccy", trade.ccy);
                cmd.Parameters.AddWithValue("@ordType", trade.ordType);
                cmd.Parameters.AddWithValue("@sz", ConvertToNullableDecimal(trade.sz));
                cmd.Parameters.AddWithValue("@state", trade.state);
                cmd.Parameters.AddWithValue("@side", trade.side);
                cmd.Parameters.AddWithValue("@px", ConvertToNullableDecimal(trade.px));
                cmd.Parameters.AddWithValue("@avgPx", ConvertToNullableDecimal(trade.avgPx));
                cmd.Parameters.AddWithValue("@accFillSz", ConvertToNullableDecimal(trade.accFillSz));
                cmd.Parameters.AddWithValue("@fee", ConvertToNullableDecimal(trade.fee));
                cmd.Parameters.AddWithValue("@feeCcy", trade.feeCcy);
                cmd.Parameters.AddWithValue("@rebate", ConvertToNullableDecimal(trade.rebate));
                cmd.Parameters.AddWithValue("@rebateCcy", trade.rebateCcy);
                cmd.Parameters.AddWithValue("@pnl", ConvertToNullableDecimal(trade.pnl));
                cmd.Parameters.AddWithValue("@posSide", trade.posSide);
                cmd.Parameters.AddWithValue("@lever", trade.lever);
                cmd.Parameters.AddWithValue("@ctVal", trade.ctVal);
                cmd.Parameters.AddWithValue("@tag", trade.tag);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task SavePageTradeFillsHistoryToDataBase(IEnumerable<OkxTradeFillsHistory> trades)
        {
            const string query = @"
INSERT INTO tradefills(instType, instId, tradeId, ordId, clOrdId, billId, subType, tag, fillPx,
                       fillSz, fillIdxPx, fillPnl, fillPxVol, fillPxUsd, fillMarkVol,
                       fillFwdPx, fillMarkPx, side, posSide, execType, feeCcy, fee, ts, fillTime, feeRate)
VALUES (@instType, @instId, @tradeId, @ordId, @clOrdId, @billId, @subType, @tag, @fillPx,
        @fillSz, @fillIdxPx, @fillPnl, @fillPxVol, @fillPxUsd, @fillMarkVol,
        @fillFwdPx, @fillMarkPx, @side, @posSide, @execType, @feeCcy, @fee, @ts, @fillTime, @feeRate)
ON DUPLICATE KEY UPDATE
instType = VALUES(instType),
instId = VALUES(instId),
tradeId = VALUES(tradeId),
ordId = VALUES(ordId),
clOrdId = VALUES(clOrdId),
subType = VALUES(subType),
tag = VALUES(tag),
fillPx = VALUES(fillPx),
fillSz = VALUES(fillSz),
fillIdxPx = VALUES(fillIdxPx),
fillPnl = VALUES(fillPnl),
fillPxVol = VALUES(fillPxVol),
fillPxUsd = VALUES(fillPxUsd),
fillMarkVol = VALUES(fillMarkVol),
fillFwdPx = VALUES(fillFwdPx),
fillMarkPx = VALUES(fillMarkPx),
side = VALUES(side),
posSide = VALUES(posSide),
execType = VALUES(execType),
feeCcy = VALUES(feeCcy),
fee = VALUES(fee),
ts = VALUES(ts),
fillTime = VALUES(fillTime),
feeRate = VALUES(feeRate);
";

            if (_mySqlConnection.State != System.Data.ConnectionState.Open)
                await _mySqlConnection.OpenAsync();
            foreach (var trade in trades)
            {
                await using var cmd = new MySqlCommand(query, _mySqlConnection);
                cmd.Parameters.AddWithValue("@instType", trade.InstType);
                cmd.Parameters.AddWithValue("@instId", trade.instId);
                cmd.Parameters.AddWithValue("@tradeId", trade.tradeId);
                cmd.Parameters.AddWithValue("@ordId", trade.ordId);
                cmd.Parameters.AddWithValue("@clOrdId", trade.clOrdId);
                cmd.Parameters.AddWithValue("@billId", trade.billId);
                cmd.Parameters.AddWithValue("@subType", trade.subType);
                cmd.Parameters.AddWithValue("@tag", trade.tag);
                cmd.Parameters.AddWithValue("@fillPx", ConvertToNullableDecimal(trade.fillPx));
                cmd.Parameters.AddWithValue("@fillSz", ConvertToNullableDecimal(trade.fillSz));
                cmd.Parameters.AddWithValue("@fillIdxPx", trade.fillIdxPx);
                cmd.Parameters.AddWithValue("@fillPnl", trade.fillPnl);
                cmd.Parameters.AddWithValue("@fillPxVol", trade.fillPxVol);
                cmd.Parameters.AddWithValue("@fillPxUsd", trade.fillPxUsd);
                cmd.Parameters.AddWithValue("@fillMarkVol", trade.fillMarkVol);
                cmd.Parameters.AddWithValue("@fillFwdPx", trade.fillFwdPx);
                cmd.Parameters.AddWithValue("@fillMarkPx", trade.fillMarkPx);
                cmd.Parameters.AddWithValue("@side", trade.side);
                cmd.Parameters.AddWithValue("@posSide", trade.posSide);
                cmd.Parameters.AddWithValue("@execType", trade.execType);
                cmd.Parameters.AddWithValue("@feeCcy", trade.feeCcy);
                cmd.Parameters.AddWithValue("@fee", ConvertToNullableDecimal(trade.fee));
                cmd.Parameters.AddWithValue("@ts", ConvertToNullableLong(trade.ts));
                cmd.Parameters.AddWithValue("@fillTime", ConvertToNullableLong(trade.fillTime));
                cmd.Parameters.AddWithValue("@feeRate", ConvertToNullableDecimal(trade.feeRate));
                await cmd.ExecuteNonQueryAsync();
            }
            await _mySqlConnection.CloseAsync();
        }

        private static decimal? ConvertToNullableDecimal(string value)
        {
            //Console.WriteLine(value);
            return decimal.TryParse(value, System.Globalization.NumberStyles.Any,
    System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : null;
        }

        private static long? ConvertToNullableLong(string value)
        {
            // Console.WriteLine(value);
            return long.TryParse(value, System.Globalization.NumberStyles.Any,
    System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : null;
        }

        public async Task ExecuteSQLQueryWithoutReturningParameters(string query)
        {
            if (_mySqlConnection.State != System.Data.ConnectionState.Open)
                await _mySqlConnection.OpenAsync();
            // Выполнение запроса

            await using var cmd = new MySqlCommand(query, _mySqlConnection);
            await cmd.ExecuteNonQueryAsync(); // ← важно!
        }

        public async Task<string> ExecuteSqlQueryReturnParamString(string query)
        {
            if (_mySqlConnection.State != System.Data.ConnectionState.Open)
                await _mySqlConnection.OpenAsync();

            await using var cmd = new MySqlCommand(query, _mySqlConnection);
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToString(result);
        }

        /// <summary>
        /// Принимает SQL запрос как параметр и возвращает ответ в виде списока
        /// </summary>
        /// <param name="query"> string SQL запрос</param>
        /// <returns> Task<IEnumerable<string>> ответ на запрос </returns>
        public async Task<IEnumerable<string>> ExecuteSqlQueryReturnParamListString(string query)
        {
            var result = new List<string>();

            if (_mySqlConnection.State != System.Data.ConnectionState.Open)
                await _mySqlConnection.OpenAsync();

            await using var cmd = new MySqlCommand(query, _mySqlConnection);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(reader.GetString(0)); // 0 — индекс столбца
            }
            return result;
        }

        public async Task<DataTable> ExecuteSqlQueryReturnDataTable(string query)
        {
            var table = new DataTable();

            if (_mySqlConnection.State != System.Data.ConnectionState.Open)
                await _mySqlConnection.OpenAsync();

            await using var cmd = new MySqlCommand(query, _mySqlConnection);
            await using var reader = await cmd.ExecuteReaderAsync();

            table.Load(reader); // загружаем все данные из reader в DataTable

            return table;
        }

        //обновить используемые торговые пары
        public async Task UpdateUniqueTradingPairsInBD()
        {
            const string query = @"INSERT IGNORE INTO `tradingpairs` (OKX)
                                    SELECT DISTINCT InstId
                                    FROM bills_table
                                    WHERE InstId IS NOT NULL;";

            await ExecuteSQLQueryWithoutReturningParameters(query);
        }

        public async Task UpdateUniqueCoinsInBD()
        {
            const string query = @"INSERT IGNORE INTO `coins` (OKX)
                                    SELECT DISTINCT ccy
                                    FROM bills_table
                                    WHERE ccy IS NOT NULL;";

            await ExecuteSQLQueryWithoutReturningParameters(query);
        }

        public async Task<IEnumerable<string>> GetUniqueCoinsAsync()
        {
            var result = new List<string>();

            if (_mySqlConnection.State != System.Data.ConnectionState.Open)
                await _mySqlConnection.OpenAsync();

            const string query = "SELECT OKX FROM coins";

            await using var cmd = new MySqlCommand(query, _mySqlConnection);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                string okxValue = reader["OKX"].ToString();

                result.Add(okxValue);
            }
            return result;
        }
    }
}
