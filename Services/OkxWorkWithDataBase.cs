using bot_analysis.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using bot_analysis.Models;
using System.Collections;
using Google.Protobuf.WellKnownTypes;
using System.Reflection.Metadata.Ecma335;

namespace bot_analysis.Services
{
    internal class OkxWorkWithDataBase :IWorkWithDataBase
    {
        private readonly MySqlConnection _mySqlConnection;

        public OkxWorkWithDataBase(MySqlConnection mySqlConnection)
        {
            _mySqlConnection = mySqlConnection;
        }


        public async Task SavePageAccountTransfersToDataBase(IEnumerable<Bill> trades)
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
            int counter = 0;
            long? temp;


            foreach (var trade in trades)
            {
                using var cmd = new MySqlCommand(query, _mySqlConnection);

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





        public async Task SavePageTradeFillsHistoryToDataBase(IEnumerable<TradeFillsHistory> trades)
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
                using var cmd = new MySqlCommand(query, _mySqlConnection);


                cmd.Parameters.AddWithValue("@instType", trade.instType);
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

        private decimal? ConvertToNullableDecimal(string value)
        {
            //Console.WriteLine(value);
            return decimal.TryParse(value, System.Globalization.NumberStyles.Any,
    System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : null;
        }
        
        private long? ConvertToNullableLong(string value)
        {
           // Console.WriteLine(value);
            return long.TryParse(value, System.Globalization.NumberStyles.Any,
    System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : null;
        }


        /// <summary>
        /// находит в таблице `bills_table` billid на 50 строк созданый ранее чем последний
        /// для перезаписи последних 50 (исключает не полное отображение данных из за
        /// загруженности системы) и записи новых
        /// </summary>
        /// <returns> значение billid в строковом представлении </returns>

        public async Task<string> SearchPointToReadNewDataForAccountTransfers()
        {
            if (_mySqlConnection.State != System.Data.ConnectionState.Open)
                await _mySqlConnection.OpenAsync();



            const string query = @"
                                    SELECT billid
                                    FROM `bills_table`
                                    ORDER BY ts DESC
                                    LIMIT 1 OFFSET 49;";

            using var cmd = new MySqlCommand(query, _mySqlConnection);

            var result = await cmd.ExecuteScalarAsync();

            return (Convert.ToString(result));

        }




        /// <summary>
        /// находит в таблице ручных сделок billid последней сделки
        /// </summary>
        /// <returns> значение billid в строковом представлении </returns>
        public async Task <string> SearcPointToReadNewDataForFillsHistory()
        {
            if (_mySqlConnection.State != System.Data.ConnectionState.Open)
                await _mySqlConnection.OpenAsync();



            const string query = @"
                                    SELECT billid
                                    FROM tradefills
                                    ORDER BY fillTime DESC
                                    LIMIT 1;";

            using var cmd = new MySqlCommand(query, _mySqlConnection);

            var result = await cmd.ExecuteScalarAsync();
            
            return (Convert.ToString(result));
            
        }


    }

}
