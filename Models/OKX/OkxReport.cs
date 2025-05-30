using System.ComponentModel.DataAnnotations;

namespace bot_analysis.Models.OKX
{
    public class OkxReport
    {
        [Display(Name = "Перечень монет", Order = 10)]
        public string Coins { get; set; } // Перечень монет
        [Display(Name = "Заведено/выведено [color:#FFD966]", Order = 15)]
        public string DepositWithdraw { get; set; } //заведено/выведено

        [Display(Name = "На какую  сумму, USDT [color:#FFD966]", Order = 15)]
        public string DepositWithdrawSum { get; set; } //заведено/выведено

        [Display(Name = "Заведено/выведено>Зачислено", Order = 20)]
        public string Deposit { get; set; }// Зачислено
        [Display(Name = "Заведено/выведено>На  какую сумму оценочно", Order = 30)]
        public string DepositSum { get; set; }//На какую сумму оценочно
        [Display(Name = "Заведено/выведено>Выведено монет", Order = 40)]
        public string Withdraw { get; internal set; }//выведено монет
        [Display(Name = "Заведено/выведено>На какую сумму оценочно", Order = 50)]
        public string WithdrawSum { get; internal set; }//На какую сумму оценочно
        [Display(Name = "Общий капитал [color:#00B0F0]", Order = 60)]
        public string Eq { get; set; }//Общий капитал
        [Display(Name = "Общий капитал>Доступно", Order = 62)]
        public string Availbal { get; set; }//Доступно

        [Display(Name = "Общий капитал>Заморожено всего", Order = 64)]
        public string Frozenbal { get; set; }//Заморожено всего

        [Display(Name = "Общий капитал>Заморожено всего>В ордерах", Order = 66)]
        public string Ordfrozen { get; set; }//В ордерах
        [Display(Name = "Общий капитал>Заморожено всего>В стратегиях (ботах)", Order = 68)]
        public string Stgyeq { get; set; }//В стратегиях (ботах)




        [Display(Name = "В USD [color:#00B0F0]", Order = 70)]
        public string Equsd { get; set; }//В USD
        [Display(Name = "Свободно на споте", Order = 90)]
        public string Cashbal { get; set; }//Свободно на споте
        [Display(Name = "На спотовом аккаунте", Order = 130)]
        public string Spotbal { get; set; }//На спотовом аккаунте
        [Display(Name = "Количество купленных монет", Order = 140)]
        public string BuyAmount { get; set; }// Количество купленных монет
        [Display(Name = "На сумму", Order = 150)]
        public string BuyTotal { get; set; }// На сумму
        [Display(Name = "Средняя цена покупки", Order = 160)]
        public string BuyAvgPrice { get; set; }// Средняя цена покупки
        [Display(Name = "Средняя цена покупки с учетом переводов", Order = 170)]
        public string BuyAvgPriceIncludingTransfers { get; set; }// Средняя цена покупки с учетом переводов
        [Display(Name = "Количество проданных монет", Order = 180)]
        public string SellAmount { get; set; }// Количество проданных монет
        [Display(Name = "На  сумму", Order = 181)]
        public string SellTotal { get; set; }// На сумму
        [Display(Name = "Средняя цена продажи", Order = 190)]
        public string SellAvgPrice { get; set; }// Средняя цена продажи
        [Display(Name = "Средняя цена продажи с учетом переводов", Order = 191)]
        public object SelAvgPriceIncludingTransfers { get; internal set; }// Средняя цена
                                                                          // продажи с учетом переводов
        [Display(Name = "% дохода", Order = 200)]
        public string ProfitPercent { get; set; }// % дохода
        [Display(Name = "Монет в наличии ", Order = 210)]

        public string CurrentAmount { get; set; }// Монет в наличии 
        [Display(Name = "Цена монеты ", Order = 220)]
        public string CurrentPrice { get; set; }// Цена монеты 
        [Display(Name = "Актив монет в USDT", Order = 230)]
        public string CurrentValueUsd { get; set; }// Актив монет в USDT
        [Display(Name = "Затрачено в USDT", Order = 240)]
        public string SpentTotalUsd { get; set; }// Затрачено в USDT
        [Display(Name = "Доход с учетом продажи монет по текущему курсу", Order = 250)]
        public string FullProfitUsd { get; set; }// Доход с учетом продажи монет по текущему курсу
        [Display(Name = "Количество купленных монет ботом", Order = 260)]
        public string BuyAmountBot { get; set; } // Количество купленных монет ботом
        [Display(Name = "На  сумму в USDT", Order = 270)]
        public string BuyTotalBot { get; set; }// купленных ботом на сумму в USDT
    }
}
