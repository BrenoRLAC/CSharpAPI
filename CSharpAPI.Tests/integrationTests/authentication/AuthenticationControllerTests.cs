using API.Domain;
using API.Domain.Auth;
using API.Utilities;
using CSharpAPI.Tests.integrationTests.Base.BaseIntegrationTest;
using System.Net;
using System.Net.Http.Json;
using System.Transactions;
using YourProject.Tests.Base;

namespace CSharpAPI.Tests.integrationTests.Authentication.AuthenticationControllerHappyPathTests;

public class AuthenticationControllerTests(AuthenticationFactory factory)
    : BaseIntegrationTest(factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    #region Account Creation 

    [Fact(DisplayName = "Success: Registering a valid new user returns 200")]
    public async Task Register_ValidUser_ReturnsOk()
    {
        var user = RegisterNewRandomUser();

        var response = await Client.PostAsJsonAsync("/user/Authentication/register", user);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ReturnApi<object>>();
        Assert.Equal("User Successfully registered!", result.Message);
    }

    [Theory(DisplayName = "Failure: Create user with weak password throws ArgumentException")]
    [InlineData("Short1!3", "Password must be between 10 characters and 100 characters")]
    [InlineData("NoNumbers!", "Password must have at least one number")]
    [InlineData("lowercase1!", "Password must have at least one uppercase letter.")]
    [InlineData("UPPERCASE1!", "Password must have at least one lowercase letter.")]
    [InlineData("NoSpecial123", "Password must have at least one special character. Example: \"! @ # $ % &\"")]

    public async Task Register_InvalidPasswords_ReturnsBadRequest(string weakPassword, string reason)
    {
        var request = new { UserName = "Test", Email = GetRandomEmail(), Password = weakPassword };

        var response = await _client.PostAsJsonAsync("/user/Authentication/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Password", content);
    }

    [Theory(DisplayName = "Failure: Register with invalid email formats returns BadRequest")]
    [InlineData("plainaddress", "Missing @ and domain")]
    [InlineData("@no-username.com", "Missing username")]
    [InlineData("email@domain..com", "Double dot in domain")]
    [InlineData("invalid-email@", "Missing domain")]
    public async Task Register_InvalidEmail_ReturnsBadRequest(string invalidEmail, string reason)
    {
        var request = new
        {
            UserName = "TestUser",
            Email = invalidEmail,
            Password = Password
        };

        var response = await Client.PostAsJsonAsync("/user/Authentication/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("E-mail is not valid", content);
    }

    [Fact(DisplayName = "Failure: Registering with an Email that already exists returns 400")]
    public async Task Register_EmailAlreadyExists_ReturnsBadRequest()
    {
        var activeEmail = await GetExistingActiveEmailAsync();
        Assert.NotNull(activeEmail);

        var request = new
        {
            UserName = GenerateUniqueUserName(), 
            Email = activeEmail,             
            Password = Password
        };

        var response = await Client.PostAsJsonAsync("/user/Authentication/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.ReadAsJsonAsync<ReturnApi<object>>();
        Assert.Equal("O E-mail já está sendo utilizado.", result.Message);
    }

    [Theory(DisplayName = "Failure: Registering with invalid UserNames returns BadRequest")]
    [InlineData("ab", "User name must be at least 3 characters.")]
    [InlineData("ThisNameIsWayTooLongForTheDatabase", "User name cannot exceed 20 characters.")]
    [InlineData("User@Name", "User name can only contain letters, numbers, and underscores.")]
    [InlineData("User Name", "User name can only contain letters, numbers, and underscores.")]
    public async Task Register_InvalidUserName_ReturnsBadRequest(string invalidName, string expectedError)
    {
        var request = new
        {
            UserName = invalidName,
            Email = GetRandomEmail(),
            Password = Password
        };

        var response = await Client.PostAsJsonAsync("/user/Authentication/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains(expectedError, content);
    }

    [Fact(DisplayName = "Success: Registering with a username that already exists returns 400")]
    public async Task Register_UserAlreadyExists_Returns400()
    {
        var activeUser = await GetExistingActiveUserAsync();
        Assert.NotNull(activeUser);

        var request = new
        {
            UserName = activeUser,
            Email = GetRandomEmail(),
            Password = Password
        };

        var response = await Client.PostAsJsonAsync("/user/Authentication/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.ReadAsJsonAsync<ReturnApi<object>>();
        Assert.Equal("O Nome de usuário já está sendo utilizado.", result.Message);
    }

    [Fact(DisplayName = "Failure: Registering with an existing email returns 400")]
    public async Task Register_WithEmailThatAlreadyExists_ReturnsBadRequest()
    {
        var activeEmail = await GetExistingActiveEmailAsync();
        Assert.NotNull(activeEmail);

        var request = new
        {
            UserName = "DuplicateUser",
            Email = activeEmail,
            Password = Password
        };

        var response = await Client.PostAsJsonAsync("/user/Authentication/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ReturnApi<object>>();
        Assert.Equal("O E-mail já está sendo utilizado.", result.Message);
    }
    #endregion

    #region Primary Authentication

    [Fact(DisplayName = "Failure: Unsupported grant type returns 400")]
    public async Task Post_InvalidGrantType_ReturnsBadRequest()
    {
        var form = CreateAuthForm("invalid_type", Email, Password);

        var resp = await _client.PostAsync("/user/Authentication", form);

        var result = await resp.ReadAsJsonAsync<ReturnApi<object>>();

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        Assert.NotNull(result);
        Assert.Contains("This grant type is not supported.", result.Message);
    }

    [Fact(DisplayName = "Failure: Wrong password return 401")]
    public async Task Post_WrongPassword_ReturnsUnauthorized()
    {
        var form = CreateAuthForm(GrantType, Email, WrongPassword);

        var resp = await _client.PostAsync("/user/Authentication", form);

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact(DisplayName = "Failure: Wrong email return 401")]
    public async Task Post_WrongEmail_ReturnsUnauthorized()
    {
        var form = CreateAuthForm(GrantType, WrongEmail, WrongPassword);

        var resp = await _client.PostAsync("/user/Authentication", form);

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    #endregion

    #region Second Authentication

    [Fact(DisplayName = "Full Flow: Temporary password login -> Reset -> Login with new password")]
    public async Task FullFlow_TemporaryPassword_Lifecycle_Succeeds()
    {
        var (email, userName, oldPassword) = await RegisterNewRandomUser();

        await SetUserPasswordExpirationAsync(email, DateTime.UtcNow, isTemporary: true);

        var loginForm = CreateAuthForm(GrantType, email, oldPassword);
        var loginResp = await _client.PostAsync("/user/Authentication", loginForm);

        Assert.Equal(HttpStatusCode.Created, loginResp.StatusCode);
        var loginResult = await loginResp.Content.ReadFromJsonAsync<ReturnApi<LoginResetTemporary>>();
        var resetToken = loginResult.Data.data.ResetPwdToken;
        Assert.NotNull(resetToken);

        var newPassword = "NewSecurePassword123!";
        var resetRequest = new ResetPasswordRequest
        {
            ResetPwdToken = resetToken,
            NewPassword = newPassword,
            RepeatPassword = newPassword
        };

        var resetResp = await _client.PutAsJsonAsync("/user/Authentication/resetPassword", resetRequest);
        Assert.Equal(HttpStatusCode.OK, resetResp.StatusCode);

        var finalLoginForm = CreateAuthForm(GrantType, email, newPassword);
        var finalResp = await _client.PostAsync("/user/Authentication", finalLoginForm);
  
        Assert.Equal(HttpStatusCode.OK, finalResp.StatusCode);

        var finalResult = await finalResp.Content.ReadFromJsonAsync<ReturnApi<SecondAuthToken>>();
        Assert.NotNull(finalResult.Data.SecondAuthenticationToken);
        Assert.Equal("Second authentication code sent to your email", finalResult.Message);
    }

    [Fact(DisplayName = "Failure: Second auth with malformed/invalid token encryption")]
    public async Task SecondAuth_InvalidToken_ReturnsBadRequest()
    {
        var form = new FormUrlEncodedContent([new("Token", "not-a-valid-encrypted-token")]);

        var resp = await _client.PostAsync("/user/Authentication/secondAuthentication/123456", form);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact(DisplayName = "Failure: Second auth with valid token but expired/missing in cache")]
    public async Task SecondAuth_ExpiredCacheToken_ReturnsBadRequest()
    {
        var fakeToken = Guid.NewGuid().ToString().EncryptCookie();

        var form = new FormUrlEncodedContent([new("Token", fakeToken)]);

        var resp = await _client.PostAsync("/user/Authentication/secondAuthentication/000000", form);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact(DisplayName = "Failure: Second auth with correct token but wrong 2FA code")]
    public async Task SecondAuth_WrongCode_ReturnsBadRequest()
    {
        var (token, _) = await GetInitialAuthToken(Email, Password);

        var form = new FormUrlEncodedContent([new("Token", token)]);
        var resp = await _client.PostAsync("/user/Authentication/secondAuthentication/999999", form);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    #endregion

    #region Password Reset & Logic
    [Fact(DisplayName = "Failure: Reset password with wrong Email")]
    public async Task ResetPassword_WrongEmail_ReturnsBadRequest()
    {
        var forgotReq = new { email = WrongEmail };
        var forgotResp = await _client.PutAsJsonAsync("/user/Authentication/forgotPassword", forgotReq);

        Assert.Equal(HttpStatusCode.BadRequest, forgotResp.StatusCode);
    }

    [Fact(DisplayName = "Failure: Reset password with mismatched repeat password")]
    public async Task ResetPassword_Mismatch_ReturnsBadRequest()
    {
        var (email, userName, password) = await RegisterNewRandomUser();

        var forgotResp = await _client.PutAsJsonAsync("/user/Authentication/forgotPassword", new { email = email });
        forgotResp.EnsureSuccessStatusCode();

        string tempPassword = await WaitForValueAsync(() => factory.EmailSpy.LastSentPassword);
        Assert.NotNull(tempPassword);

        var resetToken = await GetResetTokenFromLogin(email, tempPassword);

        var resetRequest = new ResetPasswordRequest
        {
            ResetPwdToken = resetToken,
            NewPassword = "NewStrongPassword123!",
            RepeatPassword = "MismatchPassword456!"
        };

        var resp = await _client.PutAsJsonAsync("/user/Authentication/resetPassword", resetRequest);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact(DisplayName = "Failure: Reset password with repeated (Older) password")]
    public async Task ResetPassword_OlderPassword_ReturnsBadRequest()
    {
        var (email, userName, password) = await RegisterNewRandomUser();

        factory.EmailSpy.ResetEmail();

        var forgotResp = await _client.PutAsJsonAsync("/user/Authentication/forgotPassword", new { email = email });
        forgotResp.EnsureSuccessStatusCode();

        string tempPassword = await WaitForValueAsync(() => factory.EmailSpy.LastSentPassword);
        Assert.NotNull(tempPassword);

        var resetToken = await GetResetTokenFromLogin(email, tempPassword);

        var resetRequest = new ResetPasswordRequest
        {
            ResetPwdToken = resetToken,
            NewPassword = "&V4Bj(-KOJGia1",
            RepeatPassword = "&V4Bj(-KOJGia1"

        };

        var resp = await _client.PutAsJsonAsync("/user/Authentication/resetPassword", resetRequest);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact(DisplayName = "Failure: Reset password with Expired Token")]
    public async Task ResetPassword_ExpiredToken_ReturnsBadRequest()
    {
        var (email, userName, password) = await RegisterNewRandomUser();

        factory.EmailSpy.ResetEmail();

        var forgotResp = await _client.PutAsJsonAsync("/user/Authentication/forgotPassword", new { email = email });
        forgotResp.EnsureSuccessStatusCode();

        string tempPassword = await WaitForValueAsync(() => factory.EmailSpy.LastSentPassword);
        var resetToken = await GetResetTokenFromLogin(email, tempPassword);

        factory.ExpireTokenInCache(resetToken);

        var request = new ResetPasswordRequest
        {
            ResetPwdToken = resetToken,
            NewPassword = "NewPassword123!",
            RepeatPassword = "NewPassword123!"
        };
        var resp = await _client.PutAsJsonAsync("/user/Authentication/resetPassword", request);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact(DisplayName = "Failure: Cannot reuse the same Reset Token twice")]
    public async Task ResetPassword_TokenReuse_ReturnsBadRequest()
    {
        var (email, userName, password) = await RegisterNewRandomUser();

        factory.EmailSpy.ResetEmail();

        await _client.PutAsJsonAsync("/user/Authentication/forgotPassword", new { email = email });
        string tempPassword = await WaitForValueAsync(() => factory.EmailSpy.LastSentPassword);
        var resetToken = await GetResetTokenFromLogin(email, tempPassword);

        var resetRequest = new ResetPasswordRequest
        {
            ResetPwdToken = resetToken,
            NewPassword = "FirstChange123!",
            RepeatPassword = "FirstChange123!"
        };

        var resp1 = await _client.PutAsJsonAsync("/user/Authentication/resetPassword", resetRequest);
        resp1.EnsureSuccessStatusCode();


        resetRequest.NewPassword = "SecondChange456!";
        resetRequest.RepeatPassword = "SecondChange456!";
        var resp2 = await _client.PutAsJsonAsync("/user/Authentication/resetPassword", resetRequest);

        Assert.Equal(HttpStatusCode.BadRequest, resp2.StatusCode);
    }

    [Fact(DisplayName = "Failure: Expired password returns 403")]
    public async Task Post_ExpiredPassword_Returns403()
    {
        var (email, userName, password) = await RegisterNewRandomUser();
       
        await SetUserPasswordExpirationAsync(email, DateTime.UtcNow.AddMinutes(-6), isTemporary: true);

        var form = CreateAuthForm(GrantType, email, Password);
        var resp = await _client.PostAsync("/user/Authentication", form);

        Assert.Equal((HttpStatusCode)403, resp.StatusCode);

        var result = await resp.Content.ReadFromJsonAsync<ReturnApi<LoginReset>>();
        
        Assert.Equal("Temporary Password Expired", result.Message);

    }

    [Fact(DisplayName = "Success: Valid temporary password returns 201 and Reset Token")]
    public async Task Post_ValidTemporaryPassword_Returns201AndToken()
    {
        var (email, userName, password) = await RegisterNewRandomUser();    

        var form = CreateAuthForm(GrantType, email, password);
        var resp = await _client.PostAsync("/user/Authentication", form);

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

        var result = await resp.Content.ReadFromJsonAsync<ReturnApi<LoginResetTemporary>>();

        Assert.Equal("Valid temporary password, update password", result.Message);

        Assert.NotNull(result.Data.data.ResetPwdToken);

        Assert.True(result.Data.data.ResetPwdToken.Length > 20);
    }

    [Fact(DisplayName = "Failure: Regular expired password returns 403 and Reset Token")]
    public async Task Post_RegularPasswordExpired_Returns403AndToken()
    {
        var (email, userName, password) = await RegisterNewRandomUser();

        await SetUserPasswordExpirationAsync(email, DateTime.UtcNow.AddDays(-91), isTemporary: false);

        var form = CreateAuthForm(GrantType, Email, Password);
        var resp = await _client.PostAsync("/user/Authentication", form);

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);

        var result = await resp.Content.ReadFromJsonAsync<ReturnApi<LoginReset>>();
        Assert.Equal("Password expired, update it", result.Message);

        Assert.NotNull(result.Data.ResetPwdToken);
    }

    #endregion


    [Fact(DisplayName = "Happy: Resend second authentication returns OK")]
    public async Task ResendSecondAuth_ReturnsOk()
    {
        var (email, userName, password) = await RegisterNewRandomUser();

        var loginForm = CreateAuthForm(GrantType, email, password);
        var resp = await _client.PostAsync("/user/Authentication", loginForm);

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

        var result = await resp.Content.ReadFromJsonAsync<ReturnApi<LoginResetTemporary>>();

        Assert.Equal("Valid temporary password, update password", result.Message);

        Assert.NotNull(result.Data.data.ResetPwdToken);

        Assert.True(result.Data.data.ResetPwdToken.Length > 20);
 
        var resetToken = result.Data.data.ResetPwdToken;
        Assert.NotNull(resetToken);        

        var newPassword = "NewSecurePassword123!";
        var resetRequest = new ResetPasswordRequest
        {
            ResetPwdToken = resetToken,
            NewPassword = newPassword,
            RepeatPassword = newPassword
        };

        var resetResp = await _client.PutAsJsonAsync("/user/Authentication/resetPassword", resetRequest);
        Assert.Equal(HttpStatusCode.OK, resetResp.StatusCode);

        var finalLoginForm = CreateAuthForm(GrantType, email, newPassword);
        var finalResp = await _client.PostAsync("/user/Authentication", finalLoginForm);

        Assert.Equal(HttpStatusCode.OK, finalResp.StatusCode);

        var finalResult = await finalResp.Content.ReadFromJsonAsync<ReturnApi<SecondAuthToken>>();
        Assert.NotNull(finalResult.Data.SecondAuthenticationToken);
        Assert.Equal("Second authentication code sent to your email", finalResult.Message);


        var mfaSessionToken = finalResult.Data.SecondAuthenticationToken;
        Assert.NotNull(mfaSessionToken);

        var resendForm = new FormUrlEncodedContent(new[]
        {
        new KeyValuePair<string, string>("secondAuthToken", mfaSessionToken)
    });

       
        var resendResp = await _client.PostAsync("/user/Authentication/resendSecondAuthentication", resendForm);

        Assert.Equal(HttpStatusCode.OK, resendResp.StatusCode);

        var resendResult = await resendResp.Content.ReadFromJsonAsync<ReturnApi<object>>();
        Assert.NotNull(resendResult);
        Assert.Equal(200, resendResult.StatusCode);
        Assert.Contains("sucesso", resendResult.Message.ToLower());
    

    }

    [Fact(DisplayName = "Happy: Forgot password for existing username returns OK")]
    public async Task ForgotPassword_ExistingUser_ReturnsOk()
    {
        var existingUsername = "627435";
        
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var resp = await _client.PutAsJsonAsync("/user/Authentication/forgotPassword", new { Username = existingUsername });
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

}