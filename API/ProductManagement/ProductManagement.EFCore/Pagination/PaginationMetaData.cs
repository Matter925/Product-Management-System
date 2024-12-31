using System.Text.Json;

namespace ProductManagement.EFCore.Pagination;

public class PaginationMetaData<T> where T : class
{
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }

    public PaginationMetaData(int totalCount, int pageSize, int currentPage, int totalPages)
    {
        TotalCount = totalCount;
        PageSize = pageSize;
        CurrentPage = currentPage;
        TotalPages = totalPages;
    }

    public static string CreatePaginationMetaData(PagedList<T> pagedList)
    {
        var metaData = new PaginationMetaData<T>(pagedList.TotalCount, pagedList.PageSize, pagedList.CurrentPage, pagedList.TotalPages);
        return JsonSerializer.Serialize(metaData);
    }
}
