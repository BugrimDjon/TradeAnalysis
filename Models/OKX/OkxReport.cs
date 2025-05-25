using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_analysis.Models.OKX
{
    public class OkxReport
    {
        public string Coins { get; set; } // Перечень монет
        public string CoinsTransf { get; set; } // Количество переведенных монет
        public string BuyAmount { get; set; }// Количество купленных монет
        public string BuyTotal { get; set; }// На сумму
        public string BuyAvgPrice { get; set; }// Средняя цена покупки
        public string SellAmount { get; set; }// Количество проданных монет
        public string SellTotal { get; set; }// На сумму
        public string SellAvgPrice { get; set; }// Средняя цена продажи
        public string ProfitPercent { get; set; }// % дохода
        public string CurrentAmount { get; set; }// Монет в наличии 
        public string CurrentPrice { get; set; }// Цена монеты 
        public string CurrentValueUsd { get; set; }// Актив монет в USDT
        public string SpentTotalUsd { get; set; }// Затрачено в USDT
        public string FullProfitUsd { get; set; }// Доход с учетом продажи монет по текущему курсу
        public string BuyAmountBot { get; set; } // Количество купленных монет ботом
        public string BuyTotalBot { get; set; }// купленных ботом на сумму в USDT
    }
}
