using ProductManagement.EFCore.IdentityModels;
using ProductManagement.EFCore.Models;
using ProductManagement.EFCore.Models;
using ProductManagement.EFCore.Shared;
using ProductManagement.Shared.Enums;

namespace ProductManagement.Services.Interfaces;

public interface IAccountService
{
    Task<ServiceResult<string?>> RegisterUser(RegisterModel model);
    Task<ServiceResult<User?>> Login(LoginModel model);
    Task<ServiceResult<User?>> TwoFactorLogin(TwoFactorLoginModel model);
    Task<bool> IsTwoFactorEnabled(string userId);
    //Task<ServiceResult<string?>> ForgotPassword(string email, bool isPortal);
    Task<ServiceResult<string?>> ChangePassword(string id, ChangePasswordModel model);
    Task<ServiceResult<string?>> ResetPassword(ResetPasswordModel model);
    Task<ServiceResult<string?>> DeleteUser(string userId);
    Task<ServiceResult<QR?>> EnableTwoFactorAuthenticationAsync(string userId, bool enable);
    Task<ServiceResult<string?>> ConfirmEmail(ConfirmEmailModel model);
    //Task<ServiceResult<string?>> SendOTPAsync(string phone, OTPTypes type);
    Task<ServiceResult<User?>> VerifyOTPAsync(string phone, string enteredOTP);
    string GenerateRandomOTP();
    Task StoreOTPInDatabaseAsync(string userId, string otp, OTPTypes type);
    Task<(string otp, DateTime expiry)> RetrieveOTPFromDatabaseAsync(string userId, string type);
    //Task<ServiceResult<IEnumerable<ApplicationUserModel>?>> GetUsersByRoleAsync(string roleName);
    Task<ServiceResult<string?>> ChangeEmailAsync(ChangeEmailModel model);
    Task<ServiceResult<string?>> ResetUserPasswordAsync(ChangePasswordByAnotherUserModel model);
    Task<ServiceResult<string?>> LockOrUnlockAccountAsync(LockOrUnlockAccountModel model);
    Task<ServiceResult<GoogleUser>> LoginWithGoogle(ApplicationUser existingUser);
    ServiceResult<GoogleUser> RegisterWithGoogle(string email, string name, string subject);
    Task<ServiceResult<GoogleUser>> RegisterAndLoginWithGoogle(string accessToken);
    //Task<ServiceResult<string?>> SendEmailConfirmation(string email, bool isPortal);
    //Task<ServiceResult<IdentityUserModel?>> Get(string id, int? ClientEntityId);
    //Task<string> SendOtp(string numbers, string message, string UserId);
    Task<User> RefreshTokenAsync(string token);
    Task<bool> RevokeTokenAsync(string token);
    Task<ServiceResult<User?>> ChangeProfile(int Id);
    Task<ServiceResult<string>> LinkAccountWithGoogle(string accessToken);
    Task<ServiceResult<GoogleUser?>> LoginWithGooglePortal(string accessToken);
    Task<DateTime> GetOtpSentCountForUser(string phone);
    Task<bool> IsPhoneNumberConfirmed();
    Task<string> GetUsersPhoneNumber(string userId);
    Task<LoginLog> CreateLoginLogAsync(ApplicationUser user);
    Task<(string, string)> GetClientPhoneNumberAndEmailByUserId(string userId);
    Task<bool> BlockUserFromCash(string userId, bool isBlocked);
    Task<bool> BlockedFromCash(string userId);
    Task<string> GetUserEmail(string userId);
    //Task<ServiceResult<string?>> RegisterClient(RegisterClientModel model);
}
