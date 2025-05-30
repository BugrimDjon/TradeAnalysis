using System.ComponentModel.DataAnnotations;

namespace bot_analysis.Models.OKX
{

    public class AvailbalClass
    {
        [Display(Name = "Свободно на споте", Order = 10)]
        public string Cashbal { get; set; }//Свободно на споте

        [Display(Name = "В ордерах", Order = 11)]
        public string Ordfrozen { get; set; }//В ордерах
        [Display(Name = "В стратегиях (ботах)", Order = 12)]
        public string Stgyeq { get; set; }//В стратегиях (ботах)
    }

    public class OkxReportV_2
    {

        public OkxReportV_2()
        {
            AvailbalDetails = new AvailbalClass();
        }



        [Display(Name = "Перечень монет", Order = 1)]
        public string Coins { get; set; } // Перечень монет
        [Display(Name = "Зачислено", Order = 2)]
        public string Deposit { get; set; }// Зачислено
        [Display(Name = "Оценочно на какую сумму", Order = 3)]
        public string DepositSum { get; set; }//Оценочно на какую сумму
        [Display(Name = "выведено монет", Order = 4)]
        public string Withdraw { get; internal set; }//выведено монет
        [Display(Name = "На какую сумму оценочно", Order = 5)]
        public string WithdrawSum { get; internal set; }//На какую сумму оценочно
        [Display(Name = "Общий капитал", Order = 6)]
        public string Eq { get; set; }//Общий капитал
        [Display(Name = "Доступно_", Order = 7)]
        public string Equsd { get; set; }//В USD

        [Display(Name = "Доступно", Order = 8)]
        public string Availbal { get; set; }//Доступно
        [Display(Name = "AvailbalDetails", Order = 9)]

        public AvailbalClass AvailbalDetails { get; set; }//Доступно

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
