using bot_analysis.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_analysis.Models
{
    public class ApiOkxBotOrder:IApiResponseWithData<OkxBotOrder>
    {
        public string code { get; set; } // Код ответа от API, "0" — успешно
        public string msg { get; set; }  // Сообщение от API (обычно пустое при успешном ответе)
        public List<OkxBotOrder> data { get; set; } // Список сделок гдид ботов

    }

    public class OkxBotOrder
    {

        public string algoId { get; set; }               // Идентификатор торгового алгоритма (бота)
        public string algoClOrdId { get; set; }          // Кастомный ID алгоритма, заданный пользователем (может быть пустым)
        public string algoOrdType { get; set; }          // Тип алгоритма (например, "grid" — сеточный бот)
        public string instType { get; set; }             // Тип инструмента (например, SPOT, SWAP, FUTURES)
        public string instId { get; set; }               // Идентификатор инструмента (например, BTC-USDT)
        public string groupId { get; set; }              // Группа ордеров в рамках алгоритма
        public string ordId { get; set; }                // Уникальный идентификатор ордера
        public string cTime { get; set; }                  // Время создания ордера (в миллисекундах с 1970-01-01)
        public string uTime { get; set; }                  // Время последнего обновления ордера (в миллисекундах)
        public string tdMode { get; set; }               // Режим торговли (например, cash, cross, isolated)
        public string ccy { get; set; }                  // Валюта маржи или расчетов (может быть пустым)
        public string ordType { get; set; }              // Тип ордера (limit, market и др.)
        public string sz { get; set; }                   // Размер ордера (объем)
        public string state { get; set; }                // Состояние ордера (filled, canceled, live и др.)
        public string side { get; set; }                 // Сторона сделки (buy или sell)
        public string px { get; set; }                   // Заданная цена ордера
        public string avgPx { get; set; }                // Средняя цена исполнения (если ордер исполнен частично или полностью)
        public string accFillSz { get; set; }            // Фактически исполненный объем
        public string fee { get; set; }                  // Комиссия, списанная за сделку
        public string feeCcy { get; set; }               // Валюта комиссии
        public string rebate { get; set; }               // Возврат (ребейт) от биржи
        public string rebateCcy { get; set; }            // Валюта ребейта
        public string pnl { get; set; }                  // Прибыль/убыток от сделки (если применимо)
        public string posSide { get; set; }              // Позиция (например, net, long, short)
        public string lever { get; set; }                // Плечо (если применимо)
        public string ctVal { get; set; }                // Стоимость контракта (для деривативов)
        public string tag { get; set; }                  // Пользовательская метка (может быть задана ботом)

    }
}
