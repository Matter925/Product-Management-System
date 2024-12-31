using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using ProductManagement.API.ResponseModule;

using ProductManagement.EFCore.Shared;
using ProductManagement.Services.Interfaces;

namespace ProductManagement.API.Controllers;

[Route("[controller]")]
[ApiController]
public class LookupsController : ControllerBase
{
    private readonly ILookupsService _lookupsService;
    private readonly ICacheService _cacheService;

    public LookupsController(ILookupsService lookupsService, ICacheService cacheService)
    {
        _lookupsService = lookupsService;
        _cacheService = cacheService;
    }

    // <summary>
    /// Retrieves table names associated with a specific category.
    /// This endpoint is accessible without authentication.
    /// </summary>
    /// <remarks>
    /// This method is decorated with the [AllowAnonymous] attribute, allowing access without authentication.
    /// It retrieves table names associated with a specific category using the <see cref="_lookupsService.GetTableNames"/> method.
    /// The method returns an HTTP 200 OK response with the retrieved table names.
    /// </remarks>
    /// <returns>An HTTP response containing the table names associated with the specified category.</returns>
    [AllowAnonymous]
    [HttpGet("GetTableNames")]
    public ActionResult GetTableNames()
    {
        return Ok(_lookupsService.GetTableNames("lookups"));
    }

    /// <summary>
    /// Retrieves data from a specified table.
    /// This endpoint is accessible without authentication.
    /// </summary>
    /// <remarks>
    /// This method is decorated with the [AllowAnonymous] attribute, allowing access without authentication.
    /// It retrieves data from a specified table using the <see cref="_lookupsService.GetAllAsync"/> method.
    /// If the data is found in the cache, the cached data is returned; otherwise, the data is fetched from the service,
    /// stored in the cache, and then returned.
    /// The method returns an HTTP 200 OK response with the retrieved data.
    /// </remarks>
    /// <param name="tableName">The name of the table to retrieve data from.</param>
    /// <returns>An HTTP response containing the data from the specified table.</returns>
    [AllowAnonymous]
    [HttpGet]
    public ActionResult Get(string tableName)
    {
        var cachedData = _cacheService.GetCachedResponse(tableName);
        if (cachedData != null)
            return Ok(JsonConvert.DeserializeObject<List<Lookups>>(cachedData));

        var rows = _lookupsService.GetAllAsync(tableName);
        _cacheService.SetCacheResponse(tableName, rows, TimeSpan.FromHours(24));
        return Ok(rows);
    }

    /// <summary>
    /// Retrieves a specific row from a specified table by its ID.
    /// This endpoint is accessible without authentication.
    /// </summary>
    /// <remarks>
    /// This method is decorated with the [AllowAnonymous] attribute, allowing access without authentication.
    /// It retrieves a specific row from a specified table using the <see cref="_lookupsService.GetByIdAsync"/> method.
    /// If the row is not found, the method returns an HTTP 404 Not Found response with an error message.
    /// If the row is found, the method returns an HTTP 200 OK response with the retrieved row data.
    /// </remarks>
    /// <param name="tableName">The name of the table to retrieve the row from.</param>
    /// <param name="id">The ID of the row to retrieve.</param>
    /// <returns>An HTTP response containing the retrieved row data or an error message if the row is not found.</returns>
    [AllowAnonymous]
    [HttpGet("{tableName}/{id}")]
    public async Task<ActionResult> Get(string tableName, int id)
    {
        var row = await _lookupsService.GetByIdAsync(tableName, id);
        if (row == null)
            return NotFound(new ApiResponse(404, $"ItemWithThisIdIsn'tFound"));

        return Ok(row);
    }

    /// <summary>
    /// Loads all lookup tables, excluding specific models, and caches them for efficient retrieval.
    /// This endpoint is accessible without authentication.
    /// </summary>
    /// <remarks>
    /// This method is decorated with the [AllowAnonymous] attribute, allowing access without authentication.
    /// It loads all lookup tables from the "lookups" schema, excluding specific models defined in the <see cref="excludedModels"/> list.
    /// For each lookup table, the method retrieves all rows using the <see cref="_lookupsService.GetAllAsync"/> method
    /// and caches the data using the <see cref="_cacheService.SetCacheResponse"/> method for efficient retrieval.
    /// The excluded models are "Categories," "Certificates," and "Classifications."
    /// The method returns an HTTP 200 OK response after caching the lookup data.
    /// </remarks>
    /// <returns>An HTTP response indicating that the lookup tables have been loaded and cached successfully.</returns>
    [AllowAnonymous]
    [HttpGet("LoadAllLookups")]
    public ActionResult LoadAllLookups()
    {
        var excludedModels = new List<string>() { "Categories", "Certificates", "Classifications" };
        var lookupTables = _lookupsService.GetTableNames("lookups");
        foreach (var lookupTable in lookupTables.Except(excludedModels))
        {
            //if (excludedModels.Contains(lookupTable)) 
            //    continue;
            var data = _lookupsService.GetAllAsync(lookupTable);
            _cacheService.SetCacheResponse(lookupTable, data, TimeSpan.FromHours(24));
        }

        return Ok();
    }


    /// \cond Developers_VERSION
    /// <summary>
    /// Creates a new record in the specified lookup table.
    /// This endpoint requires users to have the "Admin" role for authorization.
    /// </summary>
    /// <remarks>
    /// This method is decorated with the [Authorize(Roles = "Admin")] attribute, restricting access to users with the "Admin" role.
    /// It accepts the name of the lookup table as <paramref name="tableName"/> and the data to be created as <paramref name="lookups"/>.
    /// The user's ID is obtained from the claim, and the <see cref="_lookupsService.CreateAsync"/> method is used to create the record.
    /// If the creation is successful, the method removes the cached response for the specified lookup table using <see cref="_cacheService.RemoveCachedResponse"/>.
    /// The method returns an HTTP 200 OK response with a success message if the creation is successful.
    /// Otherwise, it returns an HTTP 400 Bad Request response with an error message.
    /// </remarks>
    /// <param name="tableName">The name of the lookup table.</param>
    /// <param name="lookups">The data to be created in the specified lookup table.</param>
    /// <returns>An HTTP response indicating the success or failure of the record creation.</returns>
    [Authorize(Roles = "Admin,CustomerExperience")]
    [HttpPost]
    public async Task<ActionResult> Create(string tableName, LookupsEdit lookups)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await _lookupsService.CreateAsync(userId!, tableName, lookups);
        if (result == null)
            return BadRequest(new ApiResponse(400, $"ErrorCreating"));

        _cacheService.RemoveCachedResponse(tableName);
        _cacheService.RemoveCachedResponse(tableName + "_Lookup");
        var rows = _lookupsService.GetAllAsync(tableName);
        _cacheService.SetCacheResponse(tableName, rows, TimeSpan.FromHours(24));

        return Ok(new ApiResponse(200, $"CreatedSuccessfully"));
    }

    /// <summary>
    /// Updates an existing record in the specified lookup table based on the provided ID.
    /// This endpoint requires users to have the "Admin" role for authorization.
    /// </summary>
    /// <remarks>
    /// This method is decorated with the [Authorize(Roles = "Admin")] attribute, restricting access to users with the "Admin" role.
    /// It accepts the name of the lookup table as <paramref name="tableName"/>, the ID of the record as <paramref name="id"/>,
    /// and the updated data as <paramref name="lookups"/>.
    /// The user's ID is obtained from the claim, and the <see cref="_lookupsService.UpdateAsync"/> method is used to update the record.
    /// If the update is successful, the method removes the cached response for the specified lookup table using <see cref="_cacheService.RemoveCachedResponse"/>.
    /// The method returns an HTTP 200 OK response with a success message if the update is successful.
    /// Otherwise, it returns an HTTP 400 Bad Request response with an error message.
    /// </remarks>
    /// <param name="tableName">The name of the lookup table.</param>
    /// <param name="id">The ID of the record to be updated.</param>
    /// <param name="lookups">The updated data for the specified record.</param>
    /// <returns>An HTTP response indicating the success or failure of the record update.</returns>
    [Authorize(Roles = "Admin,CustomerExperience")]
    [HttpPut]
    public async Task<ActionResult> Update(string tableName, int id, LookupsEdit lookups)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await _lookupsService.UpdateAsync(userId!, tableName, id, lookups);
        if (result == null)
            return BadRequest(new ApiResponse(400, $"ErrorUpdatingWithThisId"));

        _cacheService.RemoveCachedResponse(tableName);
        _cacheService.RemoveCachedResponse(tableName + "_Lookup");
        var rows = _lookupsService.GetAllAsync(tableName);
        _cacheService.SetCacheResponse(tableName, rows, TimeSpan.FromHours(24));
        return Ok(new ApiResponse(200, $"ItemIsUpdatedSuccessfully"));
    }

    /// <summary>
    /// Deletes a record from the specified lookup table based on the provided ID.
    /// This endpoint requires users to have the "Admin" role for authorization.
    /// </summary>
    /// <remarks>
    /// This method is decorated with the [Authorize(Roles = "Admin")] attribute, restricting access to users with the "Admin" role.
    /// It accepts the name of the lookup table as <paramref name="tableName"/> and the ID of the record to be deleted as <paramref name="id"/>.
    /// The user's ID is obtained from the claim, and the <see cref="_lookupsService.DeleteAsync"/> method is used to delete the record.
    /// If the deletion is successful, the method removes the cached response for the specified lookup table using <see cref="_cacheService.RemoveCachedResponse"/>.
    /// The method returns an HTTP 200 OK response with a success message if the deletion is successful.
    /// Otherwise, it returns an HTTP 400 Bad Request response with an error message.
    /// </remarks>
    /// <param name="tableName">The name of the lookup table.</param>
    /// <param name="id">The ID of the record to be deleted.</param>
    /// <returns>An HTTP response indicating the success or failure of the record deletion.</returns>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{tableName}/{id}")]
    public async Task<ActionResult> Delete(string tableName, int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await _lookupsService.DeleteAsync(userId!, tableName, id);
        if (result == null)
            return BadRequest(new ApiResponse(400, $"ErrorDeletingWithThisId"));

        _cacheService.RemoveCachedResponse(tableName);
        _cacheService.RemoveCachedResponse(tableName + "_Lookup");
        var rows = _lookupsService.GetAllAsync(tableName);
        _cacheService.SetCacheResponse(tableName, rows, TimeSpan.FromHours(24));
        return Ok(new ApiResponse(200, $"ItemIsDeletedSuccessfully"));
    }
}
