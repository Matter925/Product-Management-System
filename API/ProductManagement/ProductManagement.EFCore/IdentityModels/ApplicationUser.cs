using Microsoft.AspNetCore.Identity;

using ProductManagement.EFCore.Models;

namespace ProductManagement.EFCore.IdentityModels;
public class ApplicationUser : IdentityUser
{
    public virtual ICollection<RefreshToken>? RefreshTokens { get; set; }
    public bool BlockedFromCash { get; set; }
}
