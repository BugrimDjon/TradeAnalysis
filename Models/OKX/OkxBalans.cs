using bot_analysis.Interfaces;

namespace bot_analysis.Models.OKX
{
    public class ApiOkxBalans : IApiResponseWithData<OkxBalans>
    {
        public string Code { get; set; } // Код ответа (0 — успех)
        public string Msg { get; set; }  // Сообщение об ошибке, если есть
        public List<OkxBalans>? data { get; set; } // Основные данные по балансу
    }

    public class OkxBalans
    {
        public string Adjeq { get; set; }  // Корректированная стоимость активов
        public string Availeq { get; set; } // Доступная стоимость активов
        public string Borrowfroz { get; set; } // Заморожено под заимствование
        public List<OkxBalansDetails> Details { get; set; } // Детали по каждой валюте
        public string Imr { get; set; } // Initial Margin Requirement — начальная маржа
        public string Isoeq { get; set; } // Изолированная стоимость активов
        public string Mgnratio { get; set; } // Соотношение маржи
        public string Mmr { get; set; } // Maintenance Margin Requirement — поддерживающая маржа
        public string Notionalusd { get; set; } // Номинальная стоимость в USD
        public string Notionalusdforborrow { get; set; } // Для расчёта заимствования
        public string Notionalusdforfutures { get; set; } // Для фьючерсов
        public string Notionalusdforoption { get; set; } // Для опционов
        public string Notionalusdforswap { get; set; } // Для свопов
        public string Ordfroz { get; set; } // Заморожено под ордера
        public string Totaleq { get; set; } // Общая стоимость активов
        public string Utime { get; set; } // Время обновления
        public string Upl { get; set; } // Нереализованная прибыль/убыток
    }

    public class OkxBalansDetails
    {
        public string Accavgpx { get; set; } // Средняя цена покупки актива
        public string Availbal { get; set; } // Доступный баланс
        public string Availeq { get; set; } // Доступная стоимость
        public string Borrowfroz { get; set; } // Заморожено под заимствование
        public string Cashbal { get; set; } // Денежный баланс
        public string Ccy { get; set; } // Валюта (например, USDT, BTC)
        public string Clspotinuseamt { get; set; } // Кол-во, занятое под copy-trading spot
        public bool Collateralenabled { get; set; } // Может ли использоваться как залог
        public bool Collateralrestrict { get; set; } // Ограничено как залог
        public string Crossliab { get; set; } // Обязательства в кросс-марже
        public string Diseq { get; set; } // Снижение стоимости
        public string Eq { get; set; } // Общая стоимость позиции
        public string Equsd { get; set; } // Стоимость в USD
        public string Fixedbal { get; set; } // Заблокированный баланс
        public string Frozenbal { get; set; } // Замороженный баланс
        public string Imr { get; set; } // Начальная маржа по активу
        public string Interest { get; set; } // Начисленные проценты
        public string Isoeq { get; set; } // Изолированная стоимость позиции
        public string Isoliab { get; set; } // Изолированные обязательства
        public string Isoupl { get; set; } // Нереализ. PnL в изол. марже
        public string Liab { get; set; } // Обязательства
        public string Maxloan { get; set; } // Максимальный заём
        public string Maxspotinuse { get; set; } // Макс. доступно под сделки на споте
        public string Mgnratio { get; set; } // Соотношение маржи по активу
        public string Mmr { get; set; } // Поддерживающая маржа
        public string Notionallever { get; set; } // Номинальное кредитное плечо
        public string Openavgpx { get; set; } // Средняя цена открытых позиций
        public string Ordfrozen { get; set; } // Заморожено под ордера
        public string Rewardbal { get; set; } // Бонусы и награды
        public string Smtsynceq { get; set; } // Синхронизация стратегии?
        public string Spotbal { get; set; } // Баланс на споте
        public string Spotcopytradingeq { get; set; } // Баланс в копи-трейдинге
        public string Spotinuseamt { get; set; } // Используемое на споте
        public string Spotisobal { get; set; } // Изолированный баланс спота
        public string Spotupl { get; set; } // Нереализ. PnL на споте
        public string Spotuplratio { get; set; } // Соотношение PnL на споте
        public string Stgyeq { get; set; } // Баланс стратегий
        public string Totalpnl { get; set; } // Общий PnL
        public string Totalpnlratio { get; set; } // Общий PnL в процентах
        public string Twap { get; set; } // Средневзвешенная цена
        public string Utime { get; set; } // Время обновления
        public string Upl { get; set; } // Нереализованная прибыль/убыток
        public string Uplliab { get; set; } // Нереализ. убытки по обязательствам
    }
}
