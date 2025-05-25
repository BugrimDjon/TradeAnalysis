using bot_analysis.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_analysis.Models.OKX
{
    public class ApiOkxCandle : IApiResponseWithData<OkxCandle>
    {
        public string code { get; set; } // Код ответа от API, "0" — успешно
        public string msg { get; set; }  // Сообщение от API (обычно пустое при успешном ответе)
        public List<OkxCandle> data { get; set; } // Список сделок гдид ботов
    }
    public class OkxCandle

    {
        public string Ts { get; set; } // Время открытия свечи, формат метки времени
                                       // Unix в миллисекундах, например 1597026383085
        public string O { get; set; } // Открытая цена
        public string H { get; set; } // самая высокая цена
        public string L { get; set; } // Самая низкая цена
        public string C { get; set; } // Цена закрытия
        public string Vol { get; set; } // Объем торговли, с единицей contract. Если это derivativesконтракт,
                                        // значение равно количеству контрактов. Если это SPOT/ MARGIN, значение
                                        // равно количеству в базовой валюте.
        public string Volccy { get; set; } // Объем торговли, с единицей currency.
                                           // Если это derivativesконтракт, о значение — это
                                           // количество базовой валюты. Если это SPOT/ MARGIN,
                                           // то значение — это количество в валюте котировки.
        public string Volccyquote { get; set; } // Объем торговли, значение — это
                                                // количество в валюте котировки,
                                                // например, единица измерения — USDT для BTC-USDT и
                                                // BTC-USDT-SWAP; единица измерения — USD для BTC-USD-SWAP
        public string Confirm { get; set; } // Состояние свечей. 0: Линия К не завершена 1: Линия К завершена


    }
}
