using bot_analysis.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_analysis.Models
{

    public class ApiOkxBill : IApiResponseWithData<OkxBill>
    {
        public string code { get; set; }
        public string msg { get; set; }
        public List<OkxBill>? data { get; set; }
    }
    public class OkxBill
    {
        // Баланс после транзакции
        public string Bal { get; set; }

        // Изменение баланса
        public string BalChg { get; set; }

        // Уникальный идентификатор счета
        public string BillId { get; set; }

        // Валюта, например USDT
        public string Ccy { get; set; }

        // Идентификатор клиентского ордера
        public string ClOrdId { get; set; }

        // Тип исполнения (например, M — Maker)
        public string ExecType { get; set; }

        // Комиссия за сделку
        public string Fee { get; set; }

        // Форвардная цена (если применимо)
        public string FillFwdPx { get; set; }

        // Индексная цена
        public string FillIdxPx { get; set; }

        // Маркет-прайс
        public string FillMarkPx { get; set; }

        // Объём по маркет-прайсу
        public string FillMarkVol { get; set; }

        // Цена сделки в USD
        public string FillPxUsd { get; set; }

        // Объём сделки по цене
        public string FillPxVol { get; set; }

        // Время исполнения сделки (в мс)
        public string FillTime { get; set; }

        // Исходный аккаунт (6: Funding, 18: Trading)
        public string From { get; set; }

        // Торговая пара, например LSK-USDT
        public string InstId { get; set; }

        // Тип инструмента, например SPOT
        public string InstType { get; set; }

        // Проценты (если начислялись)
        public string Interest { get; set; }

        // Режим маржи
        public string MgnMode { get; set; }

        // Заметки или комментарии
        public string Notes { get; set; }

        // Идентификатор ордера
        public string OrdId { get; set; }

        // Прибыль/убыток
        public string Pnl { get; set; }

        // Баланс по позиции
        public string PosBal { get; set; }

        // Изменение баланса по позиции
        public string PosBalChg { get; set; }

        // Цена сделки
        public string Px { get; set; }

        // Подтип операции
        public string SubType { get; set; }

        // Объём сделки
        public string Sz { get; set; }

        // Метка/тег
        public string Tag { get; set; }

        // Получающий аккаунт (6: Funding, 18: Trading)
        public string To { get; set; }

        // Идентификатор сделки
        public string TradeId { get; set; }

        // Метка времени (в мс)
        public string Ts { get; set; }

        // Тип счета (1 = перевод, 2 = торговля и т.п.)
        public string Type { get; set; }
    }

}
