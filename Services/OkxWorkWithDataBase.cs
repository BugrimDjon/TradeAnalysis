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

        public async Task SaveTradeFillsHistoryToDataBase(IEnumerable<TradeFillsHistory> trades)
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
            return decimal.TryParse(value, System.Globalization.NumberStyles.Any,
    System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : null;
        }
        
        private long? ConvertToNullableLong(string value)
        {
            return long.TryParse(value, System.Globalization.NumberStyles.Any,
    System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : null;
        }

        /// <summary>
        /// находит в таблице ручных сделок billid последней сделки
        /// </summary>
        /// <returns> значение billid в строковом представлении </returns>
        public async Task <string> SearchLastTradeFillsHistoryFromDB()
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
