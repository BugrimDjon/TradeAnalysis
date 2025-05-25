using bot_analysis.Enums;
using bot_analysis.Models.OKX;

namespace bot_analysis.Interfaces
{
    public interface ITradeApiClient
    {
        //получить Json по указанному пути urlPath
        Task<string> GetPageJsonAsync(
                                    string urlPath,
                                    PaginationDirection? direction = null,
                                    string? point = null
                                    );

        // универсальный метод для запроса по API
        Task<IEnumerable<TData>> GetApiDataAsync<TResponse, TData>(
                                        OkxEndpointInfo endPointData,
                                        PaginationDirection? afterBefore = null,
                                        string? pointRead = null
                                        )
             where TResponse : IApiResponseWithData<TData>;
    }
}
