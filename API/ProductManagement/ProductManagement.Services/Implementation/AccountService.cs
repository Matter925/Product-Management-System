using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Web;

using AutoMapper;

using Google.Apis.Auth;
using Google.Authenticator;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using ProductManagement.EFCore.Models;
using ProductManagement.EFCore.Shared;

using ProductManagement.EFCore.Abstractions;
using ProductManagement.EFCore.Enums;
using ProductManagement.EFCore.IdentityModels;
using ProductManagement.Services.Interfaces;
using ProductManagement.Shared.Enums;
using ProductManagement.EFCore.Models;

namespace ProductManagement.Services.Implementation;

public class AccountService(UserManager<ApplicationUser> userManager, ITokenService tokenService, SignInManager<ApplicationUser> signInManager, IEmailService emailSettings, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IOTPService oTPService, RoleManager<IdentityRole> roleManager, IMapper mapper, IUnitOfWork unitOfWork) : IAccountService
{

    /// <summary>
    /// Sends a One-Time Password (OTP) to the provided email address for phone number confirmation.
    /// </summary>
    /// <param name="email">The email address associated with the user account.</param>
    /// <returns>
    /// A <see cref="ServiceResult{T}"/> containing a status code and optional message:
    /// - 200: OTP sent successfully.
    /// - 400: The provided email address is not associated with any account,
    ///        or the phone number has already been confirmed, or an error occurred during OTP sending.
    /// </returns>
    //public async Task<ServiceResult<string?>> SendOTPAsync(string phone, OTPTypes type)
    //{
    //    var user = await userManager.Users.SingleOrDefaultAsync(u => u.PhoneNumber == phone);
    //    if (user == null)
    //        return new ServiceResult<string?>(400, "TheProvidedPhoneNumberIsNotAssociatedWithAnyAccount", null);

    //    var otp = GenerateRandomOTP();
    //    await StoreOTPInDatabaseAsync(user.Id, otp, type);
    //    var sendResult = await SendOtp(user.PhoneNumber, otp, user.Id);
    //    if (sendResult == "100")
    //        return new ServiceResult<string?>(200, null, "AnOtpHasBeenSuccessfullySentToYourPhoneNumber");
    //    else
    //        return new ServiceResult<string?>(400, null, sendResult);
    //}

    /// <summary>
    /// Verifies the entered One-Time Password (OTP) for phone number confirmation.
    /// </summary>
    /// <param name="email">The email address associated with the user account.</param>
    /// <param name="enteredOTP">The OTP entered by the user.</param>
    /// <returns>
    /// A <see cref="ServiceResult{T}"/> containing a status code and optional message:
    /// - 200: Phone number has been successfully verified.
    /// - 400: The provided email address is not associated with any account,
    ///        or the phone number has already been confirmed,
    ///        or the entered OTP is incorrect or has expired.
    /// </returns>
    public async Task<ServiceResult<User?>> VerifyOTPAsync(string phone, string enteredOTP)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
        if (user == null)
            return new ServiceResult<User?>(400, "TheProvidedPhoneNumberIsNotAssociatedWithAnyAccount", null);

        (var storedOTP, var expiry) = await RetrieveOTPFromDatabaseAsync(user.Id, OTPTypes.Login.ToString());
        var isValid = storedOTP == enteredOTP && expiry > DateTime.UtcNow;
        if (isValid)
        {
            user.PhoneNumberConfirmed = true;
            await userManager.UpdateAsync(user);
            var logs = await CreateLoginLogAsync(user);
            var userToReturn = new User
            {
                UserId = user.Id,
                Token = await tokenService.CreateTokenAsync(user, userManager),
                LoginLog = logs
            };
            var refreshToken = GenerateRefreshToken();
            userToReturn.RefreshToken = refreshToken.Token;
            userToReturn.RefreshTokenExpiration = refreshToken.ExpiresOn;
            user.RefreshTokens ??= new List<RefreshToken>();
            user.RefreshTokens.Add(refreshToken);
            await userManager.UpdateAsync(user);
            return new ServiceResult<User?>(200, null, userToReturn);
        }
        return new ServiceResult<User?>(400, "TheOtpYouEnteredIsIncorrectOrHasExpired", null);
    }

    /// <summary>
    /// Generates a random One-Time Password (OTP) of a specified length.
    /// </summary>
    /// <returns>A randomly generated OTP string.</returns>
    public string GenerateRandomOTP()
    {
        const int otpLength = 4;
        const string otpChars = "0123456789";
        var random = new Random();
        var otp = new char[otpLength];
        for (var i = 0; i < otpLength; i++)
            otp[i] = otpChars[random.Next(otpChars.Length)];

        return new string(otp);
    }

    /// <summary>
    /// Stores the generated OTP along with its expiration time in the database for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user for whom the OTP is generated.</param>
    /// <param name="otp">The One-Time Password (OTP) to be stored.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StoreOTPInDatabaseAsync(string userId, string otp, OTPTypes type)
    {
        var userOTP = new UsersOTP()
        {
            UserId = userId,
            OTP = otp,
            ExpiryTime = DateTime.UtcNow.AddMinutes(5),
            Type = type.ToString(),
            IP = httpContextAccessor?.HttpContext?.Items["IpAddress"] as string
        };
        await oTPService.CreateAsync(userOTP);
    }

    /// <summary>
    /// Retrieves the count of OTPs sent and the latest OTP's expiry time for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user for whom OTP information is retrieved.</param>
    /// <returns>A tuple containing the count of OTPs sent and the latest OTP's expiry time.</returns>
    public async Task<DateTime> GetOtpSentCountForUser(string phone)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
        var expiryTime = unitOfWork.Repository<UsersOTP>().GetQueryable<UsersOTP>(u => u.UserId == user.Id).OrderByDescending(u => u.ExpiryTime).Select(u => u.ExpiryTime).FirstOrDefault();
        return expiryTime;
    }

    // <summary>
    /// Retrieves the last OTP and its expiry time for a specific user from the database.
    /// </summary>
    /// <param name="userId">The ID of the user for whom OTP information is retrieved.</param>
    /// <returns>A tuple containing the last OTP and its expiry time.</returns>
    public async Task<(string otp, DateTime expiry)> RetrieveOTPFromDatabaseAsync(string userId, string type)
    {
        return await oTPService.RetrieveLastOTPFromDatabaseAsync(userId, type);
    }

    /// <summary>
    /// Registers a new user based on the provided registration model.
    /// </summary>
    /// <param name="model">The registration model containing user details.</param>
    /// <returns>
    /// A service result containing the outcome of the registration process, including any error messages or the user's ID on success.
    /// </returns>
    public async Task<ServiceResult<string?>> RegisterUser(RegisterModel model)
    {
        if (model.Role == ProductManagement.Shared.Enums.Roles.Admin)
            return new ServiceResult<string?>(400, "", null);

        var user = await userManager.FindByEmailAsync(model.Email);
        if (user != null)
            return new ServiceResult<string?>(400, "ThisEmailAddressIsAlreadyInUse", null);

        user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber
        };
        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            var errorMessages = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
            return new ServiceResult<string?>(400, errorMessages, null);
        }

        result = await userManager.AddToRoleAsync(user, model.Role.ToString());
        if (!result.Succeeded)
        {
            var errorMessages = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
            return new ServiceResult<string?>(400, errorMessages, null);
        }

        return new ServiceResult<string?>(200, null, user.Id);
    }

    /// <summary>
    /// Sends an SMS with the specified message to the provided phone numbers.
    /// </summary>
    /// <param name="numbers">The phone numbers to which the SMS should be sent.</param>
    /// <param name="message">The content of the SMS.</param>
    /// <returns>
    /// If SMS functionality is active, it sends the SMS using the configured API key and returns the result.
    /// Otherwise, it returns the original message.
    /// </returns>
    //public async Task<string> SendOtp(string numbers, string message, string userId)
    //{
    //    if (int.Parse(configuration["SmsIsActive"]) == 1)
    //    {
    //        var client = await unitOfWork.Repository<Client>()
    //            .GetQueryable<Client>(p => p.UserId == userId)
    //            .Select(p => new { p.UserId, p.LanguageId })
    //            .FirstOrDefaultAsync();

    //        if (client != null)
    //        {
    //            var otpMessage = client.LanguageId == (int)Languages.English
    //                ? $"Your ProductManagement OTP code is {message}.\r\nThanks for choosing ProductManagement."
    //                : $"الرقم السري المؤقت للدخول إلى منصة ويلنس ماب: {message}\r\nشكرا لاختيارك ويلنس ماب.";

    //            var sms = new SMS
    //            {
    //                Message = otpMessage,
    //                Numbers = numbers
    //            };
    //            return await moraService.SendSms(sms);
    //        }
    //    }
    //    return message;
    //}

    /// <summary>
    /// Confirms the user's email using the provided token.
    /// </summary>
    /// <param name="model">The model containing email and confirmation token.</param>
    /// <returns>The result of the email confirmation operation.</returns>
    public async Task<ServiceResult<string?>> ConfirmEmail(ConfirmEmailModel model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return new ServiceResult<string?>(400, "TheProvidedEmailAddressIsNotAssociatedWithAnyAccount", null);

        var result = await userManager.ConfirmEmailAsync(user, model.Token);
        if (!result.Succeeded)
            return new ServiceResult<string?>(400, "EmailConfirmationFailedTheProvidedTokenIsEitherInvalidOrHasAlreadyBeenUsed", null);

        return new ServiceResult<string?>(200, null, "Email confirmed successfully.");
    }

    /// <summary>
    /// Creates a login log entry for the specified user.
    /// </summary>
    /// <param name="user">The user for whom the login log is created.</param>
    /// <returns>The created login log entry.</returns>
    public async Task<LoginLog> CreateLoginLogAsync(ApplicationUser user)
    {
        var ipAddress = httpContextAccessor?.HttpContext?.Items["IpAddress"] as string;
        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();
        var logs = new LoginLog()
        {
            UserId = user.Id,
            LoginDate = DateTime.UtcNow,
            Ip = ipAddress,
            Role = role,
            Email = user.Email,
            Name = GetUserName(user.Id, role ?? "")
        };
       // await logsService.CreateAsync(logs);
        return logs;
    }

    /// <summary>
    /// Logs in the user based on the provided login model.
    /// </summary>
    /// <param name="model">The login model containing user credentials.</param>
    /// <returns>A service result with the logged-in user information or an error message.</returns>
    public async Task<ServiceResult<User?>> Login(LoginModel model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return new ServiceResult<User?>(400, "InvalidEmailOrPassword", null);

        if (user.LockoutEnabled && user.LockoutEnd > DateTimeOffset.UtcNow)
            return new ServiceResult<User?>(400, "AccountLockedPleaseTryAgainLater", null);

        var userRole = await userManager.GetRolesAsync(user);

        var result = await signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded)
            return new ServiceResult<User?>(400, "InvalidEmailOrPassword", null);

        var isTwoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user);
        if (isTwoFactorEnabled)
            return new ServiceResult<User?>(400, "TwoFactorAuthenticationCodeRequired", null);

        var userToReturn = await CreateUserToReturn(user);
        userToReturn.IsPhoneNumberConfirmed = user.PhoneNumberConfirmed;
        return new ServiceResult<User?>(200, null, userToReturn);
    }

    /// <summary>
    /// Creates a user object with the necessary information for return after a successful login.
    /// </summary>
    /// <param name="user">The ApplicationUser for which to create the User object.</param>
    /// <returns>A User object with relevant information for the client.</returns>
    private async Task<User> CreateUserToReturn(ApplicationUser user)
    {
        var logs = await CreateLoginLogAsync(user);
        var userToReturn = new User
        {
            UserId = user.Id,
            Token = await tokenService.CreateTokenAsync(user, userManager),
            LoginLog = logs
        };

        var refreshToken = GenerateRefreshToken();
        userToReturn.RefreshToken = refreshToken.Token;
        userToReturn.RefreshTokenExpiration = refreshToken.ExpiresOn;
        user.RefreshTokens ??= new List<RefreshToken>();
        user.RefreshTokens.Add(refreshToken);
        await userManager.UpdateAsync(user);

        return userToReturn;
    }

    /// <summary>
    /// Refreshes the access token using the provided refresh token.
    /// </summary>
    /// <param name="token">The refresh token to use for refreshing the access token.</param>
    /// <returns>A User object with the new access token and refresh token information.</returns>
    public async Task<User> RefreshTokenAsync(string token)
    {
        var newUser = new User();
        var user = await userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.RefreshTokens != null && u.RefreshTokens.Any(t => t.Token == token));

        if (user == null)
            return newUser;

        var refreshToken = user.RefreshTokens?.FirstOrDefault(t => t.Token == token);
        if (refreshToken == null || !refreshToken.IsActive)
            return newUser;

        refreshToken.RevokedOn = DateTime.UtcNow;
        await RemoveExpiredAndRevokedTokensAsync();

        var newRefreshToken = GenerateRefreshToken();
        user.RefreshTokens ??= new List<RefreshToken>();
        user.RefreshTokens.Add(newRefreshToken);

        await userManager.UpdateAsync(user);

        newUser.Token = await tokenService.CreateTokenAsync(user, userManager);
        newUser.RefreshToken = newRefreshToken.Token;
        newUser.RefreshTokenExpiration = newRefreshToken.ExpiresOn;

        return newUser;
    }

    /// <summary>
    /// Removes expired and revoked refresh tokens from the database.
    /// </summary>
    public async Task RemoveExpiredAndRevokedTokensAsync()
    {
        var expiredAndRevokedTokens = unitOfWork.Repository<RefreshToken>().GetQueryable<RefreshToken>(t => t.ExpiresOn <= DateTime.UtcNow || t.RevokedOn != null);
        foreach (var token in expiredAndRevokedTokens)
            await unitOfWork.Repository<RefreshToken>().HardDeleteAsync(token);

        await unitOfWork.CompleteAsync();
    }

    /// <summary>
    /// Revokes the specified refresh token for the user.
    /// </summary>
    /// <param name="token">The refresh token to revoke.</param>
    /// <returns><c>true</c> if the token is successfully revoked; otherwise, <c>false</c>.</returns>
    public async Task<bool> RevokeTokenAsync(string token)
    {
        var user = await userManager.Users.SingleOrDefaultAsync(U => U.RefreshTokens != null && U.RefreshTokens.Any(t => t.Token == token));
        if (user == null)
            return false;

        var refreshToken = user.RefreshTokens?.Single(t => t.Token == token);
        if (refreshToken == null || !refreshToken.IsActive)
            return false;

        refreshToken.RevokedOn = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
        return true;
    }

    /// <summary>
    /// Generates a new refresh token.
    /// </summary>
    /// <returns>A new <see cref="RefreshToken"/>.</returns>
    private static RefreshToken GenerateRefreshToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);

        return new RefreshToken()
        {
            Token = Convert.ToBase64String(bytes),
            ExpiresOn = DateTime.UtcNow.AddDays(10),
            CreatedOn = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Performs two-factor login for the user.
    /// </summary>
    /// <param name="model">The model containing login information.</param>
    /// <returns>A <see cref="ServiceResult{T}"/> containing the user information or an error message.</returns>
    public async Task<ServiceResult<User?>> TwoFactorLogin(TwoFactorLoginModel model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return new ServiceResult<User?>(400, "InvalidEmailOrPassword", null);

        var result = await signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded)
            return new ServiceResult<User?>(400, "InvalidEmailOrPassword", null);

        var tfa = new TwoFactorAuthenticator();
        var isCodeValid = tfa.ValidateTwoFactorPIN(user.Id, model.Code, TimeSpan.FromSeconds(30));
        if (!isCodeValid)
            return new ServiceResult<User?>(400, "InvalidTwoFactorAuthenticationCode", null);

        var userToReturn = await CreateUserToReturn(user);
        userToReturn.IsPhoneNumberConfirmed = user.PhoneNumberConfirmed;
        return new ServiceResult<User?>(200, null, userToReturn);
    }

    /// <summary>
    /// Checks if two-factor authentication is enabled for the specified user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns><c>true</c> if two-factor authentication is enabled; otherwise, <c>false</c>.</returns>
    public async Task<bool> IsTwoFactorEnabled(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user != null && await userManager.GetTwoFactorEnabledAsync(user);
    }

    /// <summary>
    /// Enables or disables two-factor authentication for the specified user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="enable"><c>true</c> to enable two-factor authentication, <c>false</c> to disable.</param>
    /// <returns>A <see cref="ServiceResult{T}"/> indicating the result of the operation.</returns>
    public async Task<ServiceResult<QR?>> EnableTwoFactorAuthenticationAsync(string userId, bool enable)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return new ServiceResult<QR?>(400, "UserNotFound", null);

        var result = await userManager.SetTwoFactorEnabledAsync(user, enable);
        if (!result.Succeeded)
            return new ServiceResult<QR?>(400, "FailedToUpdateTwo-FactorAuthenticationStatus", null);

        if (enable)
        {
            var tfa = new TwoFactorAuthenticator();
            var setupInfo = tfa.GenerateSetupCode("ProductManagement", user.Email, user.Id, false);
            var qrCodeImageUrl = setupInfo.QrCodeSetupImageUrl;
            var secretKey = setupInfo.ManualEntryKey;
            await userManager.SetAuthenticationTokenAsync(user, "ProductManagement", user.Id, secretKey);

            return new ServiceResult<QR?>(200, null, new QR
            {
                QRCodeImage = qrCodeImageUrl,
                SecretKey = secretKey
            });
        }
        return new ServiceResult<QR?>(200, null, new QR
        {
            QRCodeImage = null,
            SecretKey = null
        });
    }

    /// <summary>
    /// Resets the password for a user using a password reset token.
    /// </summary>
    /// <param name="model">The <see cref="ResetPasswordModel"/> containing the necessary information.</param>
    /// <returns>A <see cref="ServiceResult{T}"/> indicating the result of the operation.</returns>
    public async Task<ServiceResult<string?>> ResetPassword(ResetPasswordModel model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return new ServiceResult<string?>(400, $"UserWithThisemailNotFound", null);

        var result = await userManager.VerifyUserTokenAsync(user, userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", model.Token);
        if (!result)
            return new ServiceResult<string?>(400, $"InvalidOrExpiredPasswordResetTokenForThisEmail", null);

        var resetResult = await userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (!resetResult.Succeeded)
            return new ServiceResult<string?>(400, $"FailedToResetPasswordForThisEmail", null);

        return new ServiceResult<string?>(200, null, "PasswordResetSuccessful");
    }

    /// <summary>
    /// Changes the password for a user.
    /// </summary>
    /// <param name="id">The ID of the user.</param>
    /// <param name="model">The <see cref="ChangePasswordModel"/> containing the necessary information.</param>
    /// <returns>A <see cref="ServiceResult{T}"/> indicating the result of the operation.</returns>
    public async Task<ServiceResult<string?>> ChangePassword(string id, ChangePasswordModel model)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
            return new ServiceResult<string?>(400, $"UserWithThisIdNotFound", null);

        var isCurrentPasswordValid = await userManager.CheckPasswordAsync(user, model.CurrentPassword);
        if (!isCurrentPasswordValid)
            return new ServiceResult<string?>(400, "CurrentPasswordIsIncorrect", null);

        await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

        return new ServiceResult<string?>(200, null, "PasswordChangedSuccessfully");
    }

    /// <summary>
    /// Deletes a user based on the provided user ID.
    /// </summary>
    /// <param name="userId">The ID of the user to be deleted.</param>
    /// <returns>A <see cref="ServiceResult{T}"/> indicating the result of the operation.</returns>
    public async Task<ServiceResult<string?>> DeleteUser(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return new ServiceResult<string?>(400, "UserNotFound", null);
        try
        {
            await userManager.DeleteAsync(user);
            return new ServiceResult<string?>(200, null, "UserDeletedSuccessfully");
        }
        catch (Exception)
        {
            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            return new ServiceResult<string?>(400, "UserAccountIsLockedOut", null);
        }
    }

    /// <summary>
    /// Gets user information based on the provided user ID and optional Client entity ID.
    /// </summary>
    /// <param name="id">The ID of the user to retrieve information for.</param>
    /// <param name="ClientEntityId">The optional ID of the Client entity.</param>
    /// <returns>A <see cref="ServiceResult{T}"/> containing the user information.</returns>
    //public async Task<ServiceResult<IdentityUserModel?>> Get(string id, int? ClientEntityId)
    //{
    //    var accountInfo = await _userManager.FindByIdAsync(id);
    //    var user = await _userManager.FindByIdAsync(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
    //    var userRole = string.Empty;
    //    IList<ApplicationUser> usersInRole;
    //    if (user != null && user != accountInfo)
    //    {
    //        var UserRoles = await _userManager.GetRolesAsync(user);
    //        userRole = UserRoles.FirstOrDefault();
    //        if (userRole == Roles.Partner.ToString())
    //        {
    //            var PartnerId = _partnersService.GetIdByUserId(user.Id);
    //            var labs = await _labsService.GetAllAsync<Lab>(false, ignorePaging: true);
    //            var labsByPartner = labs.Where(n => n.PartnerId == PartnerId).ToList();
    //            usersInRole = await _userManager.GetUsersInRoleAsync(Roles.Lab.ToString());
    //            usersInRole = usersInRole.Where(user => labsByPartner.Any(labs => labs.UserId == user.Id)).ToList();
    //            accountInfo = usersInRole.FirstOrDefault(y => y.Id == id);
    //            if (accountInfo == null)
    //            {
    //                var nurses = await _nursesService.GetAllAsync<Nurse>(false, ignorePaging: true);
    //                var nursesByPartner = nurses.Where(n => n.Lab.PartnerId == PartnerId).ToList();
    //                usersInRole = await _userManager.GetUsersInRoleAsync(Roles.Nurse.ToString());
    //                usersInRole = usersInRole.Where(user => nursesByPartner.Any(nurse => nurse.UserId == user.Id)).ToList();
    //                accountInfo = usersInRole.FirstOrDefault(y => y.Id == id);
    //            }
    //        }
    //        else if (userRole == Roles.Lab.ToString())
    //        {
    //            var LabId = _labsService.GetIdByUserId(user.Id);

    //            var nurses = await _nursesService.GetAllAsync<Nurse>(false, ignorePaging: true);
    //            var nursesByPartner = nurses.Where(n => n.LabId == LabId).ToList();
    //            usersInRole = await _userManager.GetUsersInRoleAsync(Roles.Nurse.ToString());
    //            usersInRole = usersInRole.Where(user => nursesByPartner.Any(nurse => nurse.UserId == user.Id)).ToList();
    //            accountInfo = usersInRole.FirstOrDefault(y => y.Id == id);
    //        }
    //        else if (userRole == Roles.Nurse.ToString())
    //            accountInfo = await _userManager.FindByIdAsync(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
    //    }

    //    var mappedEntity = _mapper.Map<IdentityUserModel>(accountInfo);
    //    if (accountInfo != null)
    //    {
    //        mappedEntity.Roles = await _userManager.GetRolesAsync(accountInfo);
    //        if (mappedEntity.Roles != null && mappedEntity.Roles.Count > 0)
    //            mappedEntity.Role = mappedEntity.Roles.FirstOrDefault()!;

    //        if (mappedEntity.Role == Roles.Admin.ToString())
    //        {
    //            mappedEntity.EntityNameEn = "ProductManagement Admin";
    //            mappedEntity.EntityNameAr = "مدير تحليلي";
    //        }
    //        else if (mappedEntity.Role == Roles.Partner.ToString())
    //        {
    //            var entityId = _partnersService.GetIdByUserId(id);
    //            if (entityId != null)
    //                mappedEntity.EntityId = (int)entityId;
    //            mappedEntity.EntityNameEn = _partnersService.GetByIdAsync(mappedEntity.EntityId).Result.NameEn;
    //            mappedEntity.EntityNameAr = _partnersService.GetByIdAsync(mappedEntity.EntityId).Result.NameAr;
    //        }
    //        else if (mappedEntity.Role == Roles.Lab.ToString())
    //        {
    //            var entityId = _labsService.GetIdByUserId(id);
    //            if (entityId != null)
    //                mappedEntity.EntityId = (int)entityId;
    //            mappedEntity.EntityNameEn = _labsService.GetByIdAsync(mappedEntity.EntityId).Result.NameEn;
    //            mappedEntity.EntityNameAr = _labsService.GetByIdAsync(mappedEntity.EntityId).Result.NameAr;
    //        }
    //        else if (mappedEntity.Role == Roles.Nurse.ToString())
    //        {
    //            var entityId = _nursesService.GetIdByUserId(id);
    //            if (entityId != null)
    //                mappedEntity.EntityId = (int)entityId;
    //            mappedEntity.EntityNameEn = _nursesService.GetByIdAsync(mappedEntity.EntityId).Result.NameEn;
    //            mappedEntity.EntityNameAr = _nursesService.GetByIdAsync(mappedEntity.EntityId).Result.NameAr;
    //        }
    //        else if (mappedEntity.Role == Roles.CustomerExperience.ToString())
    //        {
    //            var entityId = _callCentersService.GetIdByUserId(id);
    //            if (entityId != null)
    //                mappedEntity.EntityId = (int)entityId;
    //            mappedEntity.EntityNameEn = _callCentersService.GetByIdAsync(mappedEntity.EntityId).Result.NameEn;
    //            mappedEntity.EntityNameAr = _callCentersService.GetByIdAsync(mappedEntity.EntityId).Result.NameAr;
    //        }
    //        else if (mappedEntity.Role == Roles.IT.ToString())
    //        {
    //            var entityId = _iTService.GetIdByUserId(id);
    //            if (entityId != null)
    //                mappedEntity.EntityId = (int)entityId;
    //            mappedEntity.EntityNameEn = _iTService.GetByIdAsync(mappedEntity.EntityId).Result.NameEn;
    //            mappedEntity.EntityNameAr = _iTService.GetByIdAsync(mappedEntity.EntityId).Result.NameAr;
    //        }
    //        else if (mappedEntity.Role == Roles.Client.ToString())
    //        {
    //            var ClientIds = _ClientsService.GetIdsByUserId(id);
    //            if (ClientIds != null && ClientEntityId != 0)
    //            {
    //                var entityId = ClientIds.Where(x => x.Equals(ClientEntityId));
    //                if (entityId != null)
    //                    mappedEntity.EntityId = (int)entityId.FirstOrDefault();
    //                mappedEntity.EntityNameEn = _ClientsService.GetByIdAsync(mappedEntity.EntityId).Result.FirstName;
    //                mappedEntity.EntityNameAr = _ClientsService.GetByIdAsync(mappedEntity.EntityId).Result.LastName;
    //                mappedEntity.PhoneNumber = _ClientsService.GetByIdAsync(mappedEntity.EntityId).Result.Phone;
    //            }
    //        }
    //    }

    //    if (accountInfo == null)
    //        return new ServiceResult<IdentityUserModel?>(404, "UserNotFound", null);

    //    return new ServiceResult<IdentityUserModel?>(404, null, mappedEntity);
    //}

    /// <summary>
    /// Gets users based on the provided role name and the current user's role.
    /// </summary>
    /// <param name="roleName">The name of the role to retrieve users for.</param>
    /// <returns>A <see cref="ServiceResult{T}"/> containing a list of users in the specified role.</returns>
    //public async Task<ServiceResult<IEnumerable<ApplicationUserModel>?>> GetUsersByRoleAsync(string roleName)
    //{
    //    var userId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
    //    var currentUser = await _userManager.FindByIdAsync(userId);
    //    if (currentUser == null)
    //        return new ServiceResult<IEnumerable<ApplicationUserModel>?>(404, $"UserNotFound", null);

    //    var targetRole = await _roleManager.FindByNameAsync(roleName);
    //    if (targetRole == null)
    //        return new ServiceResult<IEnumerable<ApplicationUserModel>?>(404, $"RoleNotFound", null);

    //    var currentUserRoles = await _userManager.GetRolesAsync(currentUser!);
    //    var currentUserRole = currentUserRoles.FirstOrDefault();
    //    IList<ApplicationUser> usersInTargetRole;
    //    int? partnerId = null;
    //    int? labId = null;
    //    usersInTargetRole = await _userManager.GetUsersInRoleAsync(roleName);
    //    if (roleName == Roles.Admin.ToString())
    //        usersInTargetRole = usersInTargetRole.Where(user => user.Email != "admin@ProductManagement.store").ToList();

    //    if (currentUserRole == Roles.Partner.ToString() && roleName == Roles.Nurse.ToString())
    //    {
    //        partnerId = _partnersService.GetIdByUserId(currentUser.Id);
    //        var nurses = await _nursesService.GetAllAsync<Nurse>(false, ignorePaging: true);
    //        usersInTargetRole = usersInTargetRole.Where(user => nurses.Any(nurse => nurse.UserId == user.Id)).ToList();
    //    }
    //    else if (currentUserRole == Roles.Partner.ToString() && roleName == Roles.Lab.ToString())
    //    {
    //        partnerId = _partnersService.GetIdByUserId(currentUser.Id);
    //        var labs = await _labsService.GetAllAsync<Lab>(false, ignorePaging: true);
    //        usersInTargetRole = usersInTargetRole.Where(user => labs.Any(lab => lab.UserId == user.Id)).ToList();
    //    }
    //    else if (currentUserRole == Roles.Lab.ToString() && roleName == Roles.Nurse.ToString())
    //    {
    //        labId = _labsService.GetIdByUserId(currentUser.Id);
    //        var nurses = await _nursesService.GetAllAsync<Nurse>(false, ignorePaging: true);
    //        usersInTargetRole = usersInTargetRole.Where(user => nurses.Any(nurse => nurse.UserId == user.Id)).ToList();
    //    }
    //    else if (currentUserRole == Roles.IT.ToString())
    //    {
    //        if (roleName != Roles.Partner.ToString() && roleName != Roles.Lab.ToString() && roleName != Roles.Nurse.ToString())
    //            return new ServiceResult<IEnumerable<ApplicationUserModel>?>(403, $"Access Denied for Role: {roleName}", null);
    //    }
    //    var usersToReturn = _mapper.Map<List<ApplicationUserModel>>(usersInTargetRole);
    //    foreach (var userToReturn in usersToReturn)
    //    {
    //        userToReturn.LastLoginDate = await _unitOfWork2.Repository<LoginLog>()
    //            .GetQueryable<LoginLog>(log => log.UserId == userToReturn.Id)
    //            .OrderByDescending(log => log.LoginDate)
    //            .Select(l => l.LoginDate)
    //            .FirstOrDefaultAsync();
    //    }

    //    return new ServiceResult<IEnumerable<ApplicationUserModel>?>(200, null, usersToReturn);
    //}

    /// <summary>
    /// Changes the email address of the user.
    /// </summary>
    /// <param name="model">The model containing information for changing the email.</param>
    /// <returns>A <see cref="ServiceResult{T}"/> indicating the result of the operation.</returns>
    public async Task<ServiceResult<string?>> ChangeEmailAsync(ChangeEmailModel model)
    {
        var user = await userManager.FindByIdAsync(model.UserId);
        if (user == null)
            return new ServiceResult<string?>(400, "UserNotFound", null);

        if (user.Email == model.NewEmail)
            return new ServiceResult<string?>(400, "TheNewEmailIsTheSameAsTheCurrentEmail", null);

        var token = await userManager.GenerateChangeEmailTokenAsync(user, model.NewEmail);
        var result = await userManager.ChangeEmailAsync(user, model.NewEmail, token);
        if (!result.Succeeded)
        {
            var errorMessages = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
            return new ServiceResult<string?>(400, errorMessages, null);
        }

        user.EmailConfirmed = false;
        await userManager.UpdateAsync(user);
        return new ServiceResult<string?>(200, null, "EmailAddressChangedSuccessfully");
    }

    /// <summary>
    /// Resets the password of a user by an administrator or another user with sufficient permissions.
    /// </summary>
    /// <param name="model">The model containing information for resetting the user's password.</param>
    /// <returns>A <see cref="ServiceResult{T}"/> indicating the result of the operation.</returns>
    public async Task<ServiceResult<string?>> ResetUserPasswordAsync(ChangePasswordByAnotherUserModel model)
    {
        var user = await userManager.FindByIdAsync(model.UserId);
        if (user == null)
            return new ServiceResult<string?>(400, "UserNotFound", null);

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, model.NewPassword);
        if (!result.Succeeded)
        {
            var errorMessages = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
            return new ServiceResult<string?>(400, errorMessages, null);
        }

        return new ServiceResult<string?>(200, null, "PasswordResetSuccessful");
    }

    /// <summary>
    /// Locks or unlocks the account of a user.
    /// </summary>
    /// <param name="model">The model containing information for locking or unlocking the user's account.</param>
    /// <returns>A <see cref="ServiceResult{T}"/> indicating the result of the operation.</returns>
    public async Task<ServiceResult<string?>> LockOrUnlockAccountAsync(LockOrUnlockAccountModel model)
    {
        var user = await userManager.FindByIdAsync(model.UserId);
        if (user == null)
            return new ServiceResult<string?>(400, "UserNotFound", null);

        var result = model.LockAccount
            ? await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue)
            : await userManager.SetLockoutEndDateAsync(user, null);

        if (!result.Succeeded)
        {
            var errorMessages = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
            return new ServiceResult<string?>(400, errorMessages, null);
        }

        var action = model.LockAccount ? "locked" : "unlocked";
        return new ServiceResult<string?>(200, null, $"Useraccount{action}successfully");
    }

    /// <summary>
    /// Registers and logs in a user using Google authentication.
    /// </summary>
    /// <param name="accessToken">The Google access token.</param>
    /// <returns>A <see cref="ServiceResult{T}"/> containing the result of the operation and user information.</returns>
    public async Task<ServiceResult<GoogleUser>> RegisterAndLoginWithGoogle(string accessToken)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings()
        {
            Audience = new List<string> { configuration["Authentication:Google:ClientId"]! }
        };

        var payload = await GoogleJsonWebSignature.ValidateAsync(accessToken, settings);
        var user = await userManager.FindByLoginAsync("Google", payload.Subject);
        if (user == null)
            return RegisterWithGoogle(payload.Email, payload.Name, payload.Subject);
        else
            return await LoginWithGoogle(user);
    }

    /// <summary>
    /// Registers a user with Google authentication.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <param name="name">The name of the user.</param>
    /// <param name="subject">The subject identifier from Google authentication.</param>
    /// <returns>A <see cref="ServiceResult{T}"/> containing the result of the registration and user information.</returns>
    public ServiceResult<GoogleUser> RegisterWithGoogle(string email, string name, string subject)
    {
        return new ServiceResult<GoogleUser>(200, null, new GoogleUser
        {
            Email = email,
            Name = name,
            Subject = subject
        });
    }

    /// <summary>
    /// Logs in a user with Google authentication.
    /// </summary>
    /// <param name="existingUser">The existing user obtained from the database.</param>
    /// <returns>A <see cref="ServiceResult{T}"/> containing the result of the login and user information.</returns>
    public async Task<ServiceResult<GoogleUser>> LoginWithGoogle(ApplicationUser existingUser)
    {
        var logs = await CreateLoginLogAsync(existingUser);
        var userToReturn = new GoogleUser
        {
            UserId = existingUser.Id,
            Token = await tokenService.CreateTokenAsync(existingUser, userManager),
            LoginLog = logs
        };
        await userManager.Users
        .Include(u => u.RefreshTokens)
        .SingleOrDefaultAsync(u => u.Id == existingUser.Id);

        var refreshToken = GenerateRefreshToken();
        userToReturn.RefreshToken = refreshToken.Token;
        userToReturn.RefreshTokenExpiration = refreshToken.ExpiresOn;
        existingUser.RefreshTokens ??= new List<RefreshToken>();
        existingUser.RefreshTokens.Add(refreshToken);
        await userManager.UpdateAsync(existingUser);
        userToReturn.IsPhoneNumberConfirmed = existingUser.PhoneNumberConfirmed;
        return new ServiceResult<GoogleUser>(200, null, userToReturn);
    }

    /// <summary>
    /// Links a user's account with Google authentication.
    /// </summary>
    /// <param name="accessToken">The Google access token.</param>
    /// <returns>A <see cref="ServiceResult{T}"/> containing the result of the account linking process.</returns>
    public async Task<ServiceResult<string>> LinkAccountWithGoogle(string accessToken)
    {
        var userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await userManager.FindByIdAsync(userId);
        var settings = new GoogleJsonWebSignature.ValidationSettings()
        {
            Audience = new List<string> { configuration["Authentication:Google:ClientId"]! }
        };

        var payload = await GoogleJsonWebSignature.ValidateAsync(accessToken, settings);

        var googleLoginInfo = await userManager.GetLoginsAsync(user);

        var googleLogin = googleLoginInfo.FirstOrDefault(l => l.LoginProvider == "Google");

        if (googleLogin == null)
        {
            var info = new UserLoginInfo("Google", payload.Subject, "Google");
            var result = await userManager.AddLoginAsync(user, info);

            if (result.Succeeded)
                return new ServiceResult<string>(200, null, "YourAccountHasBeenSuccessfullyLinkedWithYourGoogleAccount");
            else
                return new ServiceResult<string>(400, null, "FailedToLinkYourGoogleAccountWithYourExistingAccount");
        }
        else
            return new ServiceResult<string>(200, null, "YourAccountIsAlreadyLinkedWithYourGoogleAccount");
    }

    /// <summary>
    /// Logs in a user using Google authentication in the portal.
    /// </summary>
    /// <param name="accessToken">The Google access token.</param>
    /// <returns>
    /// A <see cref="ServiceResult{T}"/> containing the result of the login process
    /// along with the Google user information if successful.
    /// </returns>
    public async Task<ServiceResult<GoogleUser?>> LoginWithGooglePortal(string accessToken)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings()
        {
            Audience = new List<string> { configuration["Authentication:Google:ClientId"]! }
        };
        var payload = await GoogleJsonWebSignature.ValidateAsync(accessToken, settings);
        var user = await userManager.FindByLoginAsync("Google", payload.Subject);
        if (user != null)
        {
            var logs = await CreateLoginLogAsync(user);
            var userToReturn = new GoogleUser
            {
                UserId = user.Id,
                Token = await tokenService.CreateTokenAsync(user, userManager),
                LoginLog = logs
            };

            await userManager.Users
                   .Include(u => u.RefreshTokens)
                   .SingleOrDefaultAsync(u => u.Id == user.Id);
            var refreshToken = GenerateRefreshToken();
            userToReturn.RefreshToken = refreshToken.Token;
            userToReturn.RefreshTokenExpiration = refreshToken.ExpiresOn;
            user.RefreshTokens ??= new List<RefreshToken>();
            user.RefreshTokens.Add(refreshToken);
            await userManager.UpdateAsync(user);

            return new ServiceResult<GoogleUser?>(200, null, userToReturn);
        }
        else
            return new ServiceResult<GoogleUser?>(400, "Your Account Is Not Linked With Google", null);
    }

    /// <summary>
    /// Changes the profile for the authenticated user.
    /// </summary>
    /// <param name="Id">The identifier for the profile change.</param>
    /// <returns>
    /// A <see cref="ServiceResult{T}"/> containing the result of the profile change process
    /// along with the updated user information if successful.
    /// </returns>
    public async Task<ServiceResult<User?>> ChangeProfile(int Id)
    {
        var user = await userManager.FindByIdAsync(httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        if (user == null)
            return new ServiceResult<User?>(404, "UserNotFound", null);

        await tokenService.CreateTokenAsync(user, userManager, Id);
        var userToReturn = new User
        {
            UserId = user.Id,
            Token = await tokenService.CreateTokenAsync(user, userManager, Id)
        };

        await userManager.Users
        .Include(u => u.RefreshTokens)
        .SingleOrDefaultAsync(u => u.Id == user.Id);

        var refreshToken = GenerateRefreshToken();
        userToReturn.RefreshToken = refreshToken.Token;
        userToReturn.RefreshTokenExpiration = refreshToken.ExpiresOn;
        user.RefreshTokens ??= new List<RefreshToken>();
        user.RefreshTokens.Add(refreshToken);
        await userManager.UpdateAsync(user);

        return new ServiceResult<User?>(200, null, userToReturn);
    }

    /// <summary>
    /// Checks if the phone number of the authenticated user is confirmed.
    /// </summary>
    /// <returns><c>true</c> if the phone number is confirmed, otherwise <c>false</c>.</returns>
    public async Task<bool> IsPhoneNumberConfirmed()
    {
        var user = await userManager.FindByIdAsync(httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        return await userManager.IsPhoneNumberConfirmedAsync(user);
    }

    public async Task<string> GetUsersPhoneNumber(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user!.PhoneNumber!;
    }
    /// <summary>
    /// Retrieves the Email  of a user based on the user ID.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>The Email  of the user.</returns>
    public async Task<string> GetUserEmail(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user!.Email!;
    }

    public async Task<(string, string)> GetClientPhoneNumberAndEmailByUserId(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user?.PhoneNumber != null && user.Email != null)
            return (user.PhoneNumber, user.Email);
        throw new Exception("");
    }

    public async Task<bool> BlockUserFromCash(string userId, bool isBlocked)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user != null)
        {
            user.BlockedFromCash = isBlocked;
            var result = await userManager.UpdateAsync(user);
            return result.Succeeded;
        }
        return false;
    }

    public async Task<bool> BlockedFromCash(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user != null)
            return user.BlockedFromCash;

        return true;
    }

    private string GetUserName(string userId, string role)
    {
        switch (role)
        {
            case nameof(ProductManagement.Shared.Enums.Roles.Admin):
                return $"{ProductManagement.Shared.Enums.Roles.Admin}";

            default:
                return "";
        }
    }

    /// <summary>
    /// Gets Clients Users .
    /// </summary>
    /// <param name="roleName">The name of the role to retrieve users for.</param>
    /// <returns>A <see cref="ServiceResult{T}"/> containing a list of users in the specified role.</returns>

    //public async Task<ServiceResult<string?>> RegisterClient(RegisterClientModel model)
    //{
    //    var user = await userManager.FindByEmailAsync(model.Email);
    //    if (user != null)
    //        return new ServiceResult<string?>(400, "This Email Address Is Already In Use", null);

    //    user = new ApplicationUser
    //    {
    //        UserName = model.Email,
    //        Email = model.Email,
    //        PhoneNumber = model.PhoneNumber
    //    };
    //    var result = await userManager.CreateAsync(user);
    //    if (!result.Succeeded)
    //    {
    //        var errorMessages = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
    //        return new ServiceResult<string?>(400, errorMessages, null);
    //    }

    //    result = await userManager.AddToRoleAsync(user, Roles.Client.ToString());
    //    if (!result.Succeeded)
    //    {
    //        var errorMessages = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
    //        return new ServiceResult<string?>(400, errorMessages, null);
    //    }

    //    return new ServiceResult<string?>(200, null, user.Id);
    //}
}
