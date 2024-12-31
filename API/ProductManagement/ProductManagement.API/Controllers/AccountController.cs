//using System.Security.Claims;

//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//using ProductManagement.API.ResponseModule;

//using ProductManagement.API.DTOs;
//using ProductManagement.EFCore.IdentityModels;
//using ProductManagement.Services.Interfaces;
//using ProductManagement.Shared.Enums;

//namespace ProductManagement.API.Controllers;
//[ApiController]
//[Route("[controller]")]
//public class AccountController : ControllerBase
//{
//    private readonly IAccountService _accountService;
//    private readonly IEmailService _emailSettings;

//    public AccountController(IAccountService accountService, IEmailService emailSettings)
//    {
//        _accountService = accountService;
//        _emailSettings = emailSettings;
//    }

//    /// <summary>
//    /// Authenticates a user and generates a JWT token based on the provided credentials.
//    /// This method verifies the user’s role and manages additional login requirements,
//    /// such as two-factor authentication if applicable.
//    /// </summary>
//    /// <param name="model">The login model containing the user's credentials (username, password, etc.).</param>
//    /// <returns>
//    /// An <see cref="ActionResult"/> containing a response with the JWT token, refresh token (if generated),
//    /// and any additional relevant login information. Returns an error message if authentication fails.
//    /// </returns>
//    [AllowAnonymous]
//    [HttpPost("Login")]
//    public async Task<ActionResult> Login(LoginModel model)
//    {
//        var result = await _accountService.Login(model);
//        if (result.Message != null)
//        {
//            if (result.Message.Contains("Two-FactorAuthenticationCode"))
//                return BadRequest(new ApiResponse(result.StatusCode, "TwoFactorAuthenticationCodeRequired"));
//            return BadRequest(new ApiResponse(result.StatusCode, result.Message));
//        }
//        if (!string.IsNullOrEmpty(result.Data?.Token))
//        {
//              if (!string.IsNullOrEmpty(result.Data?.RefreshToken))
//                SetRefreshTokenForDashboard(result.Data.RefreshToken, result.Data.RefreshTokenExpiration);

//            return Ok(result.Data);
//        }
//        return BadRequest(new ApiResponse(400));
//    }

//    /// <summary>
//    /// Refreshes the user's JWT token by validating the stored refresh token from the cookies.
//    /// If the refresh token is valid and not expired, a new JWT token and refresh token are generated.
//    /// </summary>
//    /// <returns>
//    /// An <see cref="IActionResult"/> containing the new JWT token, along with a new refresh token set in cookies.
//    /// Returns a 400 Bad Request if the refresh token is invalid or missing.
//    /// </returns>
//    [HttpGet("refreshToken")]
//    public async Task<IActionResult> RefreshToken()
//    {
//        var refreshToken = Request.Cookies["RefreshToken"];
//        if (refreshToken == null)
//            return BadRequest("InvalidToken");

//        var result = await _accountService.RefreshTokenAsync(refreshToken);
//        if (result.RefreshToken == null)
//            return BadRequest("InvalidToken");
//        SetRefreshToken(result.RefreshToken, result.RefreshTokenExpiration);

//        return Ok(result);
//    }
//    /// <summary>
//    /// Revokes the stored refresh token, preventing its further use in obtaining new JWT tokens.
//    /// This action is typically used when a user logs out or when the token is suspected to be compromised.
//    /// </summary>
//    /// <returns>
//    /// An <see cref="IActionResult"/> indicating the success or failure of the token revocation process.
//    /// Returns a 400 Bad Request if the refresh token is missing or invalid.
//    /// </returns>

//    [HttpPost("RevokeToken")]
//    public async Task<IActionResult> RevokeToken()
//    {
//        var token = Request.Cookies["RefreshToken"];
//        if (string.IsNullOrEmpty(token))
//            return BadRequest("TokenIsRequired");

//        var result = await _accountService.RevokeTokenAsync(token);
//        if (!result)
//            return BadRequest("TokenIsInvalid");

//        return Ok();
//    }

//    /// <summary>
//    /// Refreshes the JWT token specifically for the dashboard by validating the stored refresh token 
//    /// from the user's cookies. If the refresh token is valid and not expired, a new JWT token is generated.
//    /// </summary>
//    /// <returns>
//    /// An <see cref="IActionResult"/> containing the refreshed JWT token for the dashboard, along 
//    /// with a new refresh token set in cookies. Returns a 400 Bad Request if the refresh token is invalid or missing.
//    /// </returns>

//    [HttpGet("DashboardRefreshToken")]
//    public async Task<IActionResult> RefreshTokenForDashboard()
//    {
//        var refreshToken = Request.Cookies["DashboardRefreshToken"];
//        if (refreshToken == null)
//            return BadRequest("InvalidToken");

//        var result = await _accountService.RefreshTokenAsync(refreshToken);
//        if (result.RefreshToken == null)
//            return BadRequest("InvalidToken");
//        SetRefreshTokenForDashboard(result.RefreshToken, result.RefreshTokenExpiration);

//        return Ok(result);
//    }

//    /// <summary>
//    /// Revokes the stored refresh token specifically for the dashboard, preventing its further use in obtaining new JWT tokens.
//    /// This action is typically performed when a user logs out from the dashboard or when the token is suspected to be compromised.
//    /// </summary>
//    /// <returns>
//    /// An <see cref="IActionResult"/> indicating the success or failure of the dashboard token revocation process.
//    /// Returns a 400 Bad Request if the refresh token is missing or invalid.
//    /// </returns>

//    [HttpPost("revokeTokenForDashboard")]
//    public async Task<IActionResult> RevokeTokenForDashboard()
//    {
//        var token = Request.Cookies["DashboardRefreshToken"];
//        if (string.IsNullOrEmpty(token))
//            return BadRequest("TokenIsRequired");

//        var result = await _accountService.RevokeTokenAsync(token);
//        if (!result)
//            return BadRequest("TokenIsInvalid");

//        return Ok();
//    }

//    /// <summary>
//    /// Authenticates a user using Google two-factor authentication. This method verifies the user's credentials
//    /// and the two-factor authentication code, generating a JWT token upon successful login.
//    /// </summary>
//    /// <param name="model">The two-factor login model containing user credentials and authentication code.</param>
//    /// <returns>
//    /// An <see cref="ActionResult"/> containing the JWT token and additional login information if authentication is successful.
//    /// Returns a 400 Bad Request with an error message if authentication fails or required tokens are missing.
//    /// </returns>

//    [AllowAnonymous]
//    [HttpPost("TwoFactorLogin")]
//    public async Task<ActionResult> TwoFactorLogin(TwoFactorLoginModel model)
//    {
//        var result = await _accountService.TwoFactorLogin(model);
//        if (result.Message != null)
//            return BadRequest(new ApiResponse(result.StatusCode, result.Message));

//        if (!string.IsNullOrEmpty(result.Data?.Token) && !string.IsNullOrEmpty(result.Data?.RefreshToken))
//        {
//            if (!model.IsPortal)
//                SetRefreshToken(result.Data.RefreshToken, result.Data.RefreshTokenExpiration);
//            else
//                SetRefreshTokenForDashboard(result.Data.RefreshToken, result.Data.RefreshTokenExpiration);
//            return Ok(result.Data);
//        }
//        return BadRequest(new ApiResponse(400));
//    }

//    /// <summary>
//    /// Determines whether two-factor authentication (2FA) is enabled for a specific user.
//    /// This method checks the user's settings to see if 2FA is active, which adds an extra layer of security to the login process.
//    /// </summary>
//    /// <param name="userId">The unique identifier of the user for whom to check 2FA status.</param>
//    /// <returns>
//    /// An <see cref="ActionResult"/> containing an anonymous object with a boolean property indicating 
//    /// whether two-factor authentication is enabled for the specified user. Returns a 200 OK response with the result.
//    /// </returns>
//    [HttpGet("IsTwoFactorEnabled")]
//    public async Task<ActionResult> IsTwoFactorEnabled(string userId)
//    {
//        var isTwoFactorEnabled = await _accountService.IsTwoFactorEnabled(userId);
//        return Ok(new { isTwoFactorEnabled });
//    }

//    /// <summary>
//    /// Enables or disables two-factor authentication for a specific user based on the provided parameters.
//    /// This method allows administrators or users to toggle the two-factor authentication feature for enhanced security.
//    /// </summary>
//    /// <param name="userId">The unique identifier of the user for whom two-factor authentication is being configured.</param>
//    /// <param name="enable">A boolean indicating whether to enable (true) or disable (false) two-factor authentication.</param>
//    /// <returns>
//    /// An <see cref="ActionResult"/> indicating the success or failure of the operation.
//    /// Returns a 400 Bad Request with an error message if the operation fails.
//    /// </returns>
//    [HttpPost("EnableTwoFactorAuthenticationAsync")]
//    public async Task<ActionResult> EnableTwoFactorAuthenticationAsync(string userId, bool enable)
//    {
//        var result = await _accountService.EnableTwoFactorAuthenticationAsync(userId, enable);
//        if (result.Message != null)
//            return BadRequest(new ApiResponse(result.StatusCode, result.Message));

//        return Ok(result.Data);
//    }

    

//    /// <summary>
//    /// Resets the user's password using the provided reset password model.
//    /// This method is typically used in scenarios where a user has forgotten their password or needs to change it for security reasons.
//    /// </summary>
//    /// <param name="model">The reset password model containing the user's details and the new password to be set.</param>
//    /// <returns>
//    /// An <see cref="ActionResult"/> indicating the success or failure of the password reset process.
//    /// Returns a 400 Bad Request with an error message if the reset process fails, 
//    /// and a 200 OK response containing the status and any relevant data if successful.
//    /// </returns>

//    [AllowAnonymous]
//    [HttpPost("ResetPassword")]
//    public async Task<ActionResult> ResetPassword(ResetPasswordModel model)
//    {
//        var result = await _accountService.ResetPassword(model);
//        if (result.Message != null)
//            return BadRequest(new ApiResponse(result.StatusCode, result.Message));

//        return Ok(new ApiResponse(200, result.Data));
//    }

//    /// <summary>
//    /// Confirms the user's email address using the provided confirmation model.
//    /// This method is typically invoked when a user clicks on a confirmation link sent to their email address.
//    /// </summary>
//    /// <param name="model">The email confirmation model containing the user's details and the confirmation token required for verification.</param>
//    /// <returns>
//    /// An <see cref="ActionResult"/> indicating the success or failure of the email confirmation process.
//    /// Returns a 400 Bad Request with an error message if the confirmation fails, 
//    /// and a 200 OK response with the status and any relevant data if successful.
//    /// </returns>

//    [HttpPost("ConfirmEmail")]
//    public async Task<ActionResult> ConfirmEmail(ConfirmEmailModel model)
//    {
//        var result = await _accountService.ConfirmEmail(model);
//        if (result.Message != null)
//            return BadRequest(new ApiResponse(result.StatusCode, result.Message));

//        return Ok(new ApiResponse(200, result.Data));
//    }

//    /// <summary>
//    /// Changes the password for a user with the specified ID.
//    /// This method allows a user to update their password by providing their current password and a new password.
//    /// It ensures that the user is authenticated before performing the operation.
//    /// </summary>
//    /// <param name="id">The unique identifier of the user whose password is being changed.</param>
//    /// <param name="model">The change password model containing the old password and the new password.</param>
//    /// <returns>
//    /// An <see cref="ActionResult"/> indicating the success or failure of the password change process.
//    /// Returns a 400 Bad Request with an error message if the change fails, 
//    /// and a 200 OK response with the status and any relevant data if successful.
//    /// </returns>

//    [Authorize]
//    [HttpPost("ChangePassword")]
//    public async Task<ActionResult> ChangePassword(string id, ChangePasswordModel model)
//    {
//        var result = await _accountService.ChangePassword(id, model);
//        if (result.Message != null)
//            return BadRequest(new ApiResponse(result.StatusCode, result.Message));

//        return Ok(new ApiResponse(200, result.Data));
//    }

//    /// <summary>
//    /// Verifies the provided One-Time Password (OTP) against the user's registered email for two-factor authentication.
//    /// This method is crucial for enhancing security by ensuring that the user possesses the OTP sent to their email before granting access.
//    /// </summary>
//    /// <param name="dto">An <see cref="OtpDto"/> object containing the user's phone number and the OTP to be verified.</param>
//    /// <returns>
//    /// An <see cref="ActionResult"/> indicating the success or failure of the OTP verification process.
//    /// Returns a 400 Bad Request with an error message if the verification fails, 
//    /// and a 200 OK response containing relevant data if successful.
//    /// Additionally, if a refresh token is provided, it is set for future authentication.
//    /// </returns>
//    //[HttpPost("VerifyOTP")]
//    //public async Task<ActionResult> VerifyOTPAsync(OtpDto dto)
//    //{
//    //    var result = await _accountService.VerifyOTPAsync(dto.PhoneNumber, dto.OTP);
//    //    if (result.Message != null)
//    //        return BadRequest(new ApiResponse(result.StatusCode, result.Message));
//    //    if (!string.IsNullOrEmpty(result.Data?.RefreshToken))
//    //        SetRefreshToken(result.Data.RefreshToken, result.Data.RefreshTokenExpiration);

//    //    return Ok(result.Data);
//    //}

//    /// <summary>
//    /// Sends a One-Time Password (OTP) to the user's registered phone number for two-factor authentication.
//    /// This method is essential for enhancing security by providing an additional verification step during login or other sensitive operations.
//    /// </summary>
//    /// <param name="phone">The phone number to which the OTP will be sent.</param>
//    /// <param name="type">The type of OTP being requested, defaulting to <see cref="OTPTypes.Login"/>. Other types may include verification or password recovery.</param>
//    /// <returns>
//    /// An <see cref="ActionResult"/> indicating the success or failure of the OTP sending process.
//    /// Returns a 400 Bad Request with an error message if the OTP sending fails, 
//    /// and a 200 OK response containing the status and any relevant data if successful.
//    /// </returns>

//    //[HttpPost("SendOTP")]
//    //public async Task<ActionResult> SendOTPAsync(string phone, OTPTypes type = OTPTypes.Login)
//    //{
//    //    var result = await _accountService.SendOTPAsync(phone, type);
//    //    if (result.Message != null)
//    //        return BadRequest(new ApiResponse(result.StatusCode, result.Message));

//    //    return Ok(new ApiResponse(200, result.Data));
//    //}


//    /// <summary>
//    /// Changes the email address associated with a user account.
//    /// This operation can only be performed by users with specific roles (Admin, Lab, Partner, CustomerExperience, IT, OperationManager).
//    /// </summary>
//    /// <param name="model">The model containing the user's ID and the new email address to be updated.</param>
//    /// <returns>
//    /// An <see cref="IActionResult"/> indicating the outcome of the email change process.
//    /// Returns a 400 Bad Request if the email change fails, including an error message. 
//    /// Returns a 200 OK response with relevant data if the operation is successful.
//    /// </returns>

//    [Authorize(Roles = "Admin")]
//    [HttpPost("change-email")]
//    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailModel model)
//    {
//        var role = User.FindFirst(ClaimTypes.Role)?.Value;
//        var result = await _accountService.ChangeEmailAsync(model);
//        if (result.Message != null)
//            return BadRequest(new ApiResponse(result.StatusCode, result.Message));

//        return Ok(new ApiResponse(200, result.Data));
//    }



//    /// <summary>
//    /// Changes the password for a specified user, initiated by an authorized user (Admin, Lab, Partner, CustomerExperience).
//    /// This allows designated roles to reset passwords on behalf of other users.
//    /// </summary>
//    /// <param name="model">The model containing details of the user whose password is being changed, along with the new password.</param>
//    /// <returns>
//    /// An <see cref="IActionResult"/> indicating the outcome of the password change operation.
//    /// Returns a 400 Bad Request if the password change fails, including an error message.
//    /// Returns a 200 OK response with relevant data if the operation is successful.
//    /// </returns>

//    [Authorize(Roles = "Admin")]
//    [HttpPost("change-password")]
//    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordByAnotherUserModel model)
//    {
//        var role = User.FindFirst(ClaimTypes.Role)?.Value;

//        var result = await _accountService.ResetUserPasswordAsync(model);
//        if (result.Message != null)
//            return BadRequest(new ApiResponse(result.StatusCode, result.Message));

//        return Ok(new ApiResponse(200, result.Data));
//    }

//    /// <summary>
//    /// Locks or unlocks a user account based on the specified action in the provided model.
//    /// This operation can only be performed by authorized roles, such as Admin, Lab, Partner, Customer Experience, IT, and Operation Manager.
//    /// </summary>
//    /// <param name="model">The model containing user details and the action to be performed (lock or unlock the account).</param>
//    /// <returns>
//    /// An <see cref="IActionResult"/> indicating the outcome of the account lock or unlock operation.
//    /// Returns a 400 Bad Request if the operation fails, including an error message.
//    /// Returns a 200 OK response with relevant data if the operation is successful.
//    /// </returns>

//    [Authorize(Roles = "Admin")]
//    [HttpPost("lock-or-unlock-account")]
//    public async Task<IActionResult> LockOrUnlockAccount([FromBody] LockOrUnlockAccountModel model)
//    {
//        var role = User.FindFirst(ClaimTypes.Role)?.Value;

//        var result = await _accountService.LockOrUnlockAccountAsync(model);
//        if (result.Message != null)
//            return BadRequest(new ApiResponse(result.StatusCode, result.Message));

//        return Ok(new ApiResponse(200, result.Data));
//    }

//    /// <summary>
//    /// Registers a new user and logs them in using Google authentication.
//    /// This method utilizes the provided Google access token to authenticate the user.
//    /// </summary>
//    /// <param name="accessToken">The Google access token obtained from the Google OAuth process, used for user authentication.</param>
//    /// <returns>
//    /// An <see cref="IActionResult"/> indicating the outcome of the registration and login process.
//    /// Returns a 400 Bad Request if the registration or login fails, including an error message.
//    /// Returns a 200 OK response with the user's authentication data if successful, including a JWT token and refresh token.
//    /// </returns>

//    [HttpPost("RegisterAndLoginWithGoogle")]
//    public async Task<IActionResult> RegisterAndLoginWithGoogle(string accessToken)
//    {
//        var result = await _accountService.RegisterAndLoginWithGoogle(accessToken);
//        if (result.Message != null)
//            return BadRequest(new ApiResponse(result.StatusCode, result.Message));

//        if (!string.IsNullOrEmpty(result.Data.Token) && result.Data.RefreshToken != null)
//            SetRefreshToken(result.Data.RefreshToken, result.Data.RefreshTokenExpiration);

//        return Ok(result.Data);
//    }

//    /// <summary>
//    /// Links the current user account with their Google account using the provided access token.
//    /// This allows the user to authenticate via Google in future login attempts.
//    /// </summary>
//    /// <param name="accessToken">The Google access token obtained from the Google OAuth process, used to authenticate the user's Google account.</param>
//    /// <returns>
//    /// An <see cref="IActionResult"/> indicating the outcome of the account linking process.
//    /// Returns a 400 Bad Request if the linking process fails, including an error message.
//    /// Returns a 200 OK response with the user's updated account information if successful.
//    /// </returns>

//    [HttpPost("LinkAccountWithGoogle")]
//    public async Task<IActionResult> LinkAccountWithGoogle(string accessToken)
//    {
//        var result = await _accountService.LinkAccountWithGoogle(accessToken);
//        if (result.Message != null)
//            return BadRequest(new ApiResponse(result.StatusCode, result.Message));

//        return Ok(result.Data);
//    }

//    /// <summary>
//    /// Authenticates a user in the portal using their Google account.
//    /// This method logs in the user with the provided Google access token and 
//    /// returns the necessary tokens for session management.
//    /// </summary>
//    /// <param name="accessToken">The Google access token obtained from the Google OAuth process, used to authenticate the user.</param>
//    /// <returns>
//    /// An <see cref="IActionResult"/> indicating the outcome of the login process.
//    /// Returns a 400 Bad Request if the login attempt fails, including an error message.
//    /// Returns a 200 OK response with the user's authentication tokens if successful.
//    /// </returns>

//    [HttpPost("LoginWithGooglePortal")]
//    public async Task<IActionResult> LoginWithGooglePortal(string accessToken)
//    {
//        var result = await _accountService.LoginWithGooglePortal(accessToken);
//        if (result.Message != null)
//            return BadRequest(new ApiResponse(result.StatusCode, result.Message));

//        if (!string.IsNullOrEmpty(result.Data?.Token) && result.Data.RefreshToken != null)
//            SetRefreshToken(result.Data.RefreshToken, result.Data.RefreshTokenExpiration);

//        return Ok(result.Data);
//    }
//    /// <summary>
//    /// Sends an email containing user feedback collected from the contact form.
//    /// This method processes the provided feedback details and sends an email using the configured email service.
//    /// </summary>
//    /// <param name="model">The contact form DTO containing user feedback details such as email, first name, last name, phone, and description.</param>
//    /// <returns>
//    /// An <see cref="IActionResult"/> indicating the outcome of the email sending process.
//    /// Returns a 200 OK response with a success message if the email is sent successfully.
//    /// </returns>

//    //[HttpPost("SendEmail")]
//    //public async Task<IActionResult> SendEmail(ContactUsFormDto model)
//    //{
//    //    var message = new EmailFromUser
//    //    {
//    //        ReplyTo = model.Email,
//    //        FirstName = model.FirstName + model.LastName,
//    //        Body = $"Phone : {model.Phone} , Body : {model.Description}",
//    //    };
//    //    await _emailSettings.SendEmailFromUser(message);

//    //    return Ok(new ApiResponse(200, "SuccessfullySend"));
//    //}


//    /// <summary>
//    /// Retrieves the count of OTPs sent and the last sent time for the authenticated user identified by their phone number.
//    /// This method allows tracking of OTP requests, helping to monitor OTP usage and potential abuse.
//    /// </summary>
//    /// <param name="phone">The phone number of the authenticated user for whom the OTP count and time are retrieved.</param>
//    /// <returns>
//    /// An <see cref="ActionResult"/> containing an object with the count and last sent time of OTPs for the user.
//    /// Returns a 200 OK response with the date of the last OTP sent.
//    /// </returns>

//    [HttpGet("GetOtpSentCountForUser")]
//    public async Task<ActionResult> GetOtpSentCountForUser(string phone)
//    {
//        var expiryTime = await _accountService.GetOtpSentCountForUser(phone);
//        return Ok(new { Date = expiryTime });
//    }

//    /// <summary>
//    /// Checks if the authenticated user's phone number has been confirmed.
//    /// This method is useful for validating the user's identity and ensuring that communication can be reliably sent to their phone number.
//    /// </summary>
//    /// <returns>
//    /// An <see cref="ActionResult"/> containing a response indicating whether the user's phone number is confirmed.
//    /// Returns a 200 OK response with a boolean value as a string ("true" or "false") indicating the confirmation status.
//    /// </returns>

//    [HttpGet("IsPhoneNumberConfirmed")]
//    public async Task<ActionResult> IsPhoneNumberConfirmed()
//    {
//        var IsPhoneNumberConfirmed = await _accountService.IsPhoneNumberConfirmed();
//        return Ok(new ApiResponse(200, IsPhoneNumberConfirmed.ToString()));
//    }

//    /// <summary>
//    /// Sets the refresh token for the dashboard in an HTTP-only cookie, enabling secure token-based authentication for user sessions.
//    /// This method ensures that the refresh token is stored securely and is inaccessible via client-side scripts, enhancing security against cross-site scripting (XSS) attacks.
//    /// </summary>
//    /// <param name="refreshToken">The refresh token to be stored in the cookie, allowing the user to obtain a new access token when the current one expires.</param>
//    /// <param name="expires">The expiration date and time for the cookie, determining how long the refresh token remains valid.</param>
//    private void SetRefreshTokenForDashboard(string refreshToken, DateTime expires)
//    {
//        var cookieOptions = new CookieOptions
//        {
//            HttpOnly = true,
//            Expires = expires,
//        };
//        Response.Cookies.Append("DashboardRefreshToken", refreshToken, cookieOptions);
//    }

//    /// <summary>
//    /// Sets the refresh token in an HTTP-only cookie for secure token-based authentication.
//    /// This method is essential for maintaining user sessions by allowing the application to request new access tokens without requiring user re-authentication.
//    /// </summary>
//    /// <param name="refreshToken">The refresh token to be securely stored in the cookie, enabling the application to obtain new access tokens when the current ones expire.</param>
//    /// <param name="expires">The expiration date and time for the cookie, which determines how long the refresh token will be valid and accessible.</param>
//    private void SetRefreshToken(string refreshToken, DateTime expires)
//    {
//        var cookieOptions = new CookieOptions
//        {
//            HttpOnly = true,
//            Expires = expires,
//        };
//        Response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);
//    }
//}
