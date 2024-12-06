using Microsoft.EntityFrameworkCore;

namespace API.Helpers;


//The PagedList<T> class represents a paginated list of items that we return to the client.
public class PagedList<T> : List<T>
{
    public PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
    {
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        PageSize = pageSize;
        TotalCount = count;
        AddRange(items);  //The list
    }

    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        //Get total count of items in source, (source is the data that comes from the db query)
        var count = await source.CountAsync();
        
        //Fetches only the items for the current page
        //Skip((pageNumber - 1) * pageSize): Skips over the items from previous pages.
        //For example, if you’re on page 2 with a page size of 10, it will skip the first 10 items.
        //Take(pageSize): Selects the next pageSize items (e.g., 10 items).
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        
        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}