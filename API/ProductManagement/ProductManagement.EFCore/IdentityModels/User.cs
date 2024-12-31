using System.Text.Json.Serialization;

using ProductManagement.EFCore.Models;

namespace ProductManagement.EFCore.IdentityModels;

public class User
{
    public string? UserId { get; set; } = default!;
    public string? Token { get; set; } = default!;
    public LoginLog? LoginLog { get; set; } = default!;

    [JsonIgnore]
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiration { get; set; }
    public bool IsPhoneNumberConfirmed { get; set; }
}

public class GoogleUser
{
    public string? UserId { get; set; } = default!;
    public string? Token { get; set; } = default!;
    public string? Email { get; set; } = default!;
    public string? Subject { get; set; } = default!;
    public string? Name { get; set; } = default!;
    public LoginLog LoginLog { get; set; } = default!;
    public bool IsPhoneNumberConfirmed { get; set; }
    [JsonIgnore]
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiration { get; set; }
}