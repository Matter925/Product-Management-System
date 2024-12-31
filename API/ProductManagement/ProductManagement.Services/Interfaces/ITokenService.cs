using Microsoft.AspNetCore.Identity;

using ProductManagement.EFCore.IdentityModels;

namespace ProductManagement.Services.Interfaces;
public interface ITokenService
{
    Task<string> CreateTokenAsync(ApplicationUser appUser, UserManager<ApplicationUser> userManager, int? id = null);
}
