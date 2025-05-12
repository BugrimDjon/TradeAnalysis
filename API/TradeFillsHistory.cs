using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_analysis.API
{
    /// <summary>
    /// Возвращает DJSON по спотовым сделкам
    /// </summary>
    /// <param name="afterBefore">  указывает направление пагинации 
    ///            принимает значение API.AfterBefore.After
    ///                               API.AfterBefore.Before</param>
    ///<param name="billId">указывает с какого billId начинать пагинацию </param>                               
    internal class TradeFillsHistory
    {
        // Получение полной истории завершенных сделок на споте
        public static Task<string> GetTradeFillsHistorySpot(API.AfterBefore? afterBefore = null, string? billId = null)
        {
            string  urlPath;
            
            if (afterBefore == null && billId == null)
            {
                urlPath = "/api/v5/trade/fills-history?instType=SPOT&limit=100";
                
            }
            else if (afterBefore != null && billId != null)
            {
                switch (afterBefore)
                {
                    case API.AfterBefore.After:
                        urlPath = $"/api/v5/trade/fills-history?instType=SPOT&limit=100&after={billId}";
                        break;
                    case API.AfterBefore.Before:
                        urlPath = $"/api/v5/trade/fills-history?instType=SPOT&limit=100&before={billId}";
                        break;
                    default:
                        throw new ArgumentException("Invalid direction.");
                }
            }
            else
            {
                throw new ArgumentException("Необходимо указать оба параметра: direction и algoId.");
            }

            return API.Get_Async(urlPath);
        }
    }
}
