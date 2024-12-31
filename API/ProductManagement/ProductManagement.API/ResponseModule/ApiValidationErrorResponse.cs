namespace ProductManagement.API.ResponseModule;

public class ApiValidationErrorResponse : ApiResponse
{
    public ApiValidationErrorResponse() : base(400)
    {
    }

    public IEnumerable<string> Errors { get; set; } = new List<string>();
}
