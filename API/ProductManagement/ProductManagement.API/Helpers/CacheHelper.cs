using ProductManagement.EFCore.ResourceParams;

namespace ProductManagement.API.Helpers;

public static class CacheHelper
{
    public static string GenerateIndexCacheKey(string _entityName, ResourceParams resourceParams)
    {
        return $"{_entityName}_PageNumber={resourceParams.PageNumber};" +
            $" PageSize={resourceParams.PageSize};" +
            $" Asc={resourceParams.Asc}; OrderBy={resourceParams.OrderBy};" +
            $" FilterQuery={resourceParams.FilterQuery};";
    }
}
