using System.ComponentModel.DataAnnotations;

namespace ProductManagement.EFCore.IdentityModels;

public class LoginModel
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class TwoFactorLoginModel
{
    [EmailAddress]
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string? Code { get; set; } = default!;
    public bool IsPortal { get; set; }
}
