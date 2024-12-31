using ProductManagement.Shared.Enums;

using ProductManagement.EFCore.Models;

namespace ProductManagement.Services.Interfaces;
public interface IOTPService : IBaseService<UsersOTP>
{
    Task<(string otp, DateTime expiry)> RetrieveLastOTPFromDatabaseAsync(string userId, string type);
}
