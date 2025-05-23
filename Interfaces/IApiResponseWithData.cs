using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_analysis.Interfaces
{
    public interface IApiResponseWithData<T>
    {
        List<T> data { get; set; }
    }
}
