using System.ComponentModel.DataAnnotations;

namespace ProductManagement.EFCore.IdentityModels;

public class ResetPasswordModel
{
    [EmailAddress]
    public string Email { get; set; } = default!;

    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
    ErrorMessage = "Password must contain at least 8 characters, one uppercase letter, one lowercase letter, one digit, and one special character.")]

    public string NewPassword { get; set; } = default!;

    public string Token { get; set; } = default!;
}
