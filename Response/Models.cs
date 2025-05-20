using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace bot_analysis.Models
{


    // Главный ответ от API OKX (всегда содержит код, сообщение и данные)
    public class GridBotResponse
    {
        public string code { get; set; } // Код ответа от API, "0" — успешно
        public string msg { get; set; }  // Сообщение от API (обычно пустое при успешном ответе)
        public List<GridBotData__OLD> Data { get; set; } // Список завершённых или активных грид-ботов
    }

    // Основной класс, представляющий один грид-бот
    public class GridBotData__OLD
    {
        public string AlgoId { get; set; }           // Уникальный идентификатор бота
        public string AlgoOrdType { get; set; }      // Тип алгоритма (обычно "grid")
        public string InstId { get; set; }           // Торговая пара, например "FET-USDT"
        public string InstType { get; set; }         // Тип инструмента (обычно "SPOT")
        public string State { get; set; }            // Состояние: running, paused, stopped и т.п.
        public string Investment { get; set; }       // Сумма инвестиций в USDT
        public string BaseSz { get; set; }           // Объём базовой валюты
        public string QuoteSz { get; set; }          // Объём валюты котировки (если есть)
        public string GridNum { get; set; }          // Количество сеток (решёток) в стратегии
        public string GridProfit { get; set; }       // Прибыль от исполнения ордеров сетки
        public string FloatProfit { get; set; }      // Нереализованная прибыль/убыток
        public string TotalPnl { get; set; }         // Общая прибыль (реализованная + нереализованная)
        public string PnlRatio { get; set; }         // Доходность в виде коэффициента (например, 0.01 = 1%)
        public string CTime { get; set; }            // Время создания (Unix Timestamp в миллисекундах)
        public string UTime { get; set; }            // Время последнего обновления
        public string StopType { get; set; }         // Причина остановки: 1 = вручную, 2 = по Take Profit/Stop Loss
        public string StopResult { get; set; }       // Результат остановки: 0 = нет результата, 1 = достигнут TP/SL
        public string CancelType { get; set; }       // Тип отмены: 1 = вручную, 3 = по системе
        public string SlTriggerPx { get; set; }      // Цена активации стоп-лосса (если задан)
        public string TpTriggerPx { get; set; }      // Цена активации тейк-профита (если задан)
        public string MaxPx { get; set; }            // Верхняя граница ценовой сетки
        public string MinPx { get; set; }            // Нижняя граница ценовой сетки
        public List<TriggerParam> triggerParams { get; set; } // Параметры запуска и остановки бота
        public List<RebateTrans> rebateTrans { get; set; }     // Информация о возвратах комиссии
    }

    // Параметры, описывающие запуск и остановку бота
    public class TriggerParam
    {
        public string triggerAction { get; set; }     // Тип действия: start или stop
        public string delaySeconds { get; set; }      // Задержка перед действием (в секундах)
        public string triggerStrategy { get; set; }   // Стратегия активации (обычно "instant")
        public string triggerType { get; set; }       // Тип триггера (manual или по условиям)
        public string stopType { get; set; }          // Тип остановки (1 = по TP/SL и т.д.), может быть null
        public string triggerTime { get; set; }       // Время срабатывания триггера
    }

    // Информация о возвратах комиссии
    public class RebateTrans
    {
        public string rebate { get; set; }           // Сумма возврата
        public string rebateCcy { get; set; }        // Валюта возврата (USDT, FET и т.п.)
    }

    public class bot_orders
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