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
        public string Deposit { get; set; }// Зачислено
        public string DepositSum { get; set; }//Оценочно на какую сумму
        public string Withdraw { get; internal set; }//выведено монет
        public string WithdrawSum { get; internal set; }//На какую сумму оценочно
        public string Eq { get; set; }//Общий капитал
        public string Equsd { get; set; }//В USD
        public string Availbal { get; set; }//Доступно
        public string Cashbal { get; set; }//Свободно на споте
        public string Ordfrozen { get; set; }//В ордерах
        public string Stgyeq { get; set; }//В стратегиях (ботах)
        public string Frozenbal { get; set; }//Заморожено всего
        public string Spotbal { get; set; }//На спотовом аккаунте
        public string BuyAmount { get; set; }// Количество купленных монет
        public string BuyTotal { get; set; }// На сумму
        public string BuyAvgPrice { get; set; }// Средняя цена покупки
        public string BuyAvgPriceIncludingTransfers { get; set; }// Средняя цена покупки с учетом переводов
        public string SellAmount { get; set; }// Количество проданных монет
        public string SellTotal { get; set; }// На сумму
        public string SellAvgPrice { get; set; }// Средняя цена продажи
        public object SelAvgPriceIncludingTransfers { get; internal set; }// Средняя цена продажи с учетом переводов
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
