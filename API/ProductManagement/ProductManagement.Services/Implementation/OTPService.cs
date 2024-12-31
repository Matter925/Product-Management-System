using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using ProductManagement.Shared.Enums;

using ProductManagement.EFCore.Abstractions;
using ProductManagement.EFCore.Models;
using ProductManagement.Services.Interfaces;
using ProductManagement.EFCore.Models;

namespace ProductManagement.Services.Implementation;

public class OTPService : BaseService<UsersOTP>, IOTPService
{
    public OTPService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, httpContextAccessor)
    {
    }

    /// <summary>
    /// Retrieves the last OTP (One-Time Password) and its expiry time from the database for the specified user.
    /// </summary>
    /// <param name="userId">The user identifier for whom to retrieve the OTP.</param>
    /// <returns>
    /// A tuple containing the retrieved OTP and its expiry time.
    /// If the OTP is found, returns (OTP, ExpiryTime), otherwise throws an exception.
    /// </returns>
    public async Task<(string otp, DateTime expiry)> RetrieveLastOTPFromDatabaseAsync(string userId, string type)
    {
        var userOTP = await _unitOfWork.Repository<UsersOTP>()
            .GetQueryable<UsersOTP>().Where(u => u.UserId == userId && u.Type == type)
            .OrderByDescending(u => u.ExpiryTime)
            .FirstOrDefaultAsync();
        if (userOTP != null)
            return (userOTP.OTP, userOTP.ExpiryTime);

        throw new Exception("User OTP not found");
    }
}
