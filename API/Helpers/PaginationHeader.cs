namespace API.Helpers;



//This class is used to hold information about the pagination (like current page, items per page, total items, and total pages).
//It's designed to be included in the response headers so the client can easily access pagination info.
public class PaginationHeader(int currentPage, int itemsPerPage, int totalItems, int totalPages)
{
    public int CurrentPage { get; set; } = currentPage;
    public int ItemsPerPage { get; set; } = itemsPerPage;
    public int TotalItems { get; set; } = totalItems;
    public int TotalPages { get; set; } = totalPages;
}