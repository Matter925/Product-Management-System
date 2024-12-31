namespace ProductManagement.EFCore.ResourceParams;

public class ResourceParams
{
    // Sorting
    public string OrderBy { get; set; } = string.Empty;
    public bool Asc { get; set; } = true;

    // Filtering
    public string? FilterQuery { get; set; }

    // Pagination
    public int PageSize { get; set; } = 40;
    public int PageNumber { get; set; } = 1;
}
