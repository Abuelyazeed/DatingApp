using System.Text.Json;
using API.Helpers;

namespace API.Extentions;


//This method adds the pagination header to the response, which includes:
public static class HttpExtentions
{
    public static void AddPaginationHeader<T>(this HttpResponse response, PagedList<T> data)
    {
        var paginationHeader = new PaginationHeader(data.CurrentPage, data.PageSize, data.TotalCount, data.TotalPages);
        
        //Make sure response is in camel case
        var jsonOptions = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        
        response.Headers.Append("Pagination", JsonSerializer.Serialize(paginationHeader, jsonOptions));
        
        //Another header for client to access
         response.Headers.Append("Access-Control-Expose-Headers", "Pagination");
    }
}