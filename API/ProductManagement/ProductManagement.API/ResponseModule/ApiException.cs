namespace ProductManagement.API.ResponseModule;

public class ApiException : ApiResponse
{
    public string? StackTrace { get; set; }

    public ApiException(int statusCode, string? message = null, string? stackTrace = null) : base(statusCode, message)
    {
        StackTrace = stackTrace;
    }
}
