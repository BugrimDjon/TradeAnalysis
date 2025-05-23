using bot_analysis.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_analysis.Models.OKX
{
    // Главный ответ от API OKX (всегда содержит код, сообщение и данные)
    public class ApiOkxBot : IApiResponseWithData<OkxBot>
    {
        public string code { get; set; } // Код ответа от API, "0" — успешно
        public string msg { get; set; }  // Сообщение от API (обычно пустое при успешном ответе)
        public List<OkxBot> data { get; set; } // Список завершённых или активных грид-ботов
    }

    // Основной класс, представляющий один грид-бот
    public class OkxBot
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
        //public List<TriggerParam> triggerParams { get; set; } // Параметры запуска и остановки бота
        //public List<RebateTrans> rebateTrans { get; set; }     // Информация о возвратах комиссии
    }
}
