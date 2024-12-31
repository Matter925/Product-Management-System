namespace ProductManagement.API.DTOs;

public class FireBaseMessageModel
{
    public string Token { get; set; } = null!;
    public string? Title { get; set; } = null;
    public string? Body { get; set; } = null;
    public string? Url { get; set; } = null;
    public string? ImageUrl { get; set; } = null;
}
