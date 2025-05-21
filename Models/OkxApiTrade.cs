using bot_analysis.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_analysis.Models
{

    public class ApiOkxTradeFillsHistory : IApiResponseWithData<OkxTradeFillsHistory>
    {
        public string code { get; set; }
        public string msg { get; set; }
        public List<OkxTradeFillsHistory>? data { get; set; }
    }

    public class OkxTradeFillsHistory
    {
        public string instType { get; set; }    //Тип инструмента
        public string instId { get; set; }      //ID инструмента
        public string tradeId { get; set; }     //ID последней сделки
        public string ordId { get; set; }       //ID ордера
        public string clOrdId { get; set; }     //ID ордера клиента, назначенный клиентом
        public string billId { get; set; }      //ID счета
        public string subType { get; set; }     //Тип транзакции
        public string tag { get; set; }         //Тег ордера
        public string fillPx { get; set; }      //Последняя заполненная цена
        public string fillSz { get; set; }      //Последнее заполненное количество
        public string fillIdxPx { get; set; }   //Индексная цена на момент исполнения сделки
                                                //Для кросс-валютных спотовых пар возвращается
                                                //индексная цена baseCcy-USDT.Например, для LTC-ETH
                                                //это поле возвращает индексную цену LTC-USDT."
        public string fillPnl { get; set; }      //Последняя заполненная прибыль и убыток, применимые к ордерам, которые имеют сделку и направлены на закрытие позиции. В других условиях всегда равно 0
        public string fillPxVol { get; set; }    //Подразумеваемая волатильность при заполнении
                                                 //Применимо только к опционам; возвращает """" для других типов инструментов"
        public string fillPxUsd { get; set; }    //Цена опциона при заполнении в единицах USD
                                                 //Применимо только к опционам; возвращает """" для других типов инструментов"
        public string fillMarkVol { get; set; }  //Отметить волатильность при заполнении
                                                 //Применимо только к опционам; возвращает """" для других типов инструментов"
        public string fillFwdPx { get; set; }    //Форвардная цена при заполнении
                                                 //Применимо только к опционам; верните """" для других типов инструментов"
        public string fillMarkPx { get; set; }   //Цена маркировки при заполнении
                                                 //Применимо к ФЬЮЧЕРСАМ, СВОПАМ, ОПЦИОНАМ"
        public string side { get; set; }        //Сторона ордера
                                                //купить
                                                //продать"
        public string posSide { get; set; }     //Сторона позиции
                                                //длинная
                                                //короткая
                                                //возвращает чистый внутренний режим."
        public string execType { get; set; }    //Ликвидность тейкер или мейкер
                                                //T: тейкер
                                                //M: мейкер
                                                //Неприменимо к системным ордерам, таким как ADL и ликвидация"
        public string feeCcy { get; set; }      //Торговый сбор или валюта скидки
        public string fee { get; set; }         //Сумма торгового сбора или скидки. Вычет торгового сбора отрицательный, например, '-0,01'; скидка положительная, например, '0,01'.
        public string ts { get; set; }          //Время генерации данных, формат временной метки Unix в миллисекундах, например, 1597026383085.
        public string fillTime { get; set; }    //Время торговли, которое совпадает с fillTime для канала ордера.
        public string feeRate { get; set; }     //Ставка комиссии. Это поле возвращается только для SPOT и MARGIN

    }

}
