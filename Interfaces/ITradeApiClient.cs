using bot_analysis.Enums;
using bot_analysis.Models.OKX;
using System.Data;

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

        // универсальный метод для запроса по API возвращает TData
        Task<IEnumerable<TData>> GetApiDataAsync<TResponse, TData>(
                                        OkxEndpointInfo endPointData,
                                        PaginationDirection? afterBefore = null,
                                        string? pointRead = null
                                        )
             where TResponse : IApiResponseWithData<TData>;

        // универсальный метод для запроса по API возвращает DataTable
        Task<DataTable> GetApiDataAsDataTableUniversalAsync(
                                OkxEndpointInfo endPointData,
                                PaginationDirection? afterBefore = null,
                                string? pointRead = null);

    }


}
