using Microsoft.AspNetCore.Mvc;

using ProductManagement.API.ResponseModule;

namespace ProductManagement.API.Controllers;

[Route("errors/{code}")]
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorHandlerController : ControllerBase
{
    /// <summary>
    /// Handles error responses by returning an appropriate result with a custom API response.
    /// </summary>
    /// <param name="code">The error code to include in the API response.</param>
    /// <returns>
    /// An appropriate result with a custom API response containing the specified error code.
    /// </returns>
    [HttpGet]
    public IActionResult Error(int code)
    {
        switch (code)
        {
            case 400:
                return BadRequest(new ApiResponse(code, "BadRequest"));
            case 401:
                return Unauthorized(new ApiResponse(code, "Unauthorized"));
            case 403:
                return Forbid("Forbidden");
            case 404:
                return NotFound(new ApiResponse(code, "NotFound"));
            case 500:
                return StatusCode(500, new ApiResponse(code, "InternalServerError"));
            default:
                return StatusCode(500, new ApiResponse(code, "UnknownError"));
        }
    }
}

