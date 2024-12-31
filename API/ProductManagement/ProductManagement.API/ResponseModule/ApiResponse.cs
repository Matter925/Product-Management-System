namespace ProductManagement.API.ResponseModule;

public class ApiResponse
{
    public ApiResponse(int statusCode, string? message = null, int id = 0)
    {
        StatusCode = statusCode;
        Message = message ?? GetDefaultMessageForStatusCode(statusCode);
        Id = id;
    }

    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public int Id { get; set; }

    private static string? GetDefaultMessageForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            200 => "TheRequestWasSuccessful",
            400 => "AbadRequestWasReceivedByTheServer",
            401 => "TheRequestRequiresAuthentication",
            404 => "TheRequestedResourceCouldNotBeFound",
            500 => "AnInternalServerErrorOccurred",
            _ => null,
        };
    }
}
