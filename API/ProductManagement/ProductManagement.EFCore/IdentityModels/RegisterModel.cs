using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;

using ProductManagement.Shared.Enums;

namespace ProductManagement.EFCore.IdentityModels;

public class RegisterModel
{
    [EmailAddress]
    public string Email { get; set; } = default!;

    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
    ErrorMessage = "Password must contain at least 8 characters, one uppercase letter, one lowercase letter, one digit, and one special character.")]
    public string Password { get; set; } = default!;

    public string? PhoneNumber { get; set; }
    public Roles Role { get; set; }
}

public class RegisterClientModel
{
    [Phone]
    public string PhoneNumber { get; set; } = null!;
    [EmailAddress]
    public string Email { get; set; } = null!;
}
