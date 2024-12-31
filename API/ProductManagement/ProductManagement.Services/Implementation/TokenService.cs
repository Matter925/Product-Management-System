using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using ProductManagement.EFCore.IdentityModels;
using ProductManagement.EFCore.Models;
using ProductManagement.Services.Interfaces;
using ProductManagement.Shared.Enums;

namespace ProductManagement.Services.Implementation;
public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly ProductManagementDBContext _context;

    public TokenService(IConfiguration config, ProductManagementDBContext context)
    {
        _config = config;
        _context = context;
    }

    /// <summary>
    /// Creates a JWT (JSON Web Token) for the specified application user, including user roles and additional claims.
    /// </summary>
    /// <param name="appUser">The application user for whom the token is created.</param>
    /// <param name="userManager">The user manager instance for managing users.</param>
    /// <param name="id">Optional parameter representing an additional identifier (e.g., Client ID).</param>
    /// <returns>An asynchronous task that represents the operation and returns the generated JWT as a string.</returns>
    public async Task<string> CreateTokenAsync(ApplicationUser appUser, UserManager<ApplicationUser> userManager, int? id = null)
    {
        var claims = new List<Claim>
    {
        new(ClaimTypes.Email, appUser.Email!),
        new(ClaimTypes.NameIdentifier, appUser.Id!)
    };

        var userRoles = await userManager.GetRolesAsync(appUser);
        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Extract configuration values
        var durationInHour = double.Parse(_config["Authentication:DurationInHour"]!);
        var secretKey = _config["Authentication:SecretForKey"]!;
        var issuer = _config["Authentication:Issuer"]!;
        var audience = _config["Authentication:Audience"]!;


        var atIndex = appUser.Email!.IndexOf("@");
        var username = appUser.Email.Substring(0, atIndex);
        claims.Add(new Claim("NameEn", username));
        claims.Add(new Claim("NameAr", username));

        // Create JWT token
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var tokenDescriptor = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            expires: DateTime.UtcNow.AddHours(durationInHour),
            claims: claims,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
            notBefore: DateTime.UtcNow
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}
