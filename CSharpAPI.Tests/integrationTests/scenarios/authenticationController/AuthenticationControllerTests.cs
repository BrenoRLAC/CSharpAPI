using API.Domain;
using API.Domain.Auth;
using API.Tests.IntegrationTests.AsyncUtils;
using API.Utilities;
using CSharpAPI.Tests.integrationTests.Base.BaseIntegrationTest;
using CSharpAPI.Tests.integrationTests.helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace CSharpAPI.Tests.integrationTests.scenarios.authentication;

[Collection("Integration Sequence")]
public class AuthenticationControllerTests(IntegrationTestFactory factory)
    : BaseIntegrationTest(factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<(string UserName, string Email, string Password)> RegisterRandomUser()
    {
        var userName = DataGenerator.GetRandomUserName();
        var email = DataGenerator.GetRandomEmail();
        var password = DataGenerator.GetRandomPassword();

        var registerResponse = await Auth.RegisterNewRandomUser(userName, email, password);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        return (userName, email, password);
    }

    private async Task<string> WaitForEmailValueAsync(string email)
    {
        var value = await AsyncUtils.WaitForValueAsync(() => factory.EmailSpy.GetValueFor(email), 30);
        value.Should().NotBeNull();
        return value;
    }

    private async Task<(string Email, string Password, string SecondAuthToken)> RegisterResetAndGetSecondAuthTokenAsync()
    {
        var (_, email, password) = await RegisterRandomUser();

        var resetToken = await Auth.GetResetTokenFromLogin(email, password);
        resetToken.Should().NotBeNull();
        var newSecurePassword =DataGenerator.GetRandomPassword();
        var resetResponse = await Auth.ResetPassword(resetToken, newSecurePassword);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await Auth.GetInitialAuthToken(email, newSecurePassword, DataGenerator.GrantType);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginPayload = await loginResponse.ReadAsJsonAsync<ReturnApi<SecondAuthToken>>();
        loginPayload?.Data?.SecondAuthenticationToken.Should().NotBeNull();
        loginPayload.Message.Should().Be("Second authentication code sent to your email");

        return (email, newSecurePassword, loginPayload.Data.SecondAuthenticationToken);
    }

    #region Account Creation 

    [Fact(DisplayName = "Success: Registering a valid new user returns 200")]
    public async Task Register_ValidUser_ReturnsOk()
    {
        var response = await Auth.RegisterNewRandomUser(DataGenerator.GetRandomUserName(), DataGenerator.GetRandomEmail(), DataGenerator.GetRandomPassword());
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ReturnApi<object>>();
        result.Message.Should().Contain("User Successfully registered!");
    }

    [Theory(DisplayName = "Failure: Create user with weak password throws ArgumentException")]
    [InlineData("Short1!3", "Password must be between 10 characters and 100 characters")]
    [InlineData("NoNumbers!", "Password must have at least one number")]
    [InlineData("lowercase1!", "Password must have at least one uppercase letter")]
    [InlineData("UPPERCASE1!", "Password must have at least one lowercase letter")]
    [InlineData("NoSpecial123", "Password must have at least one special character")]
    public async Task Register_InvalidPasswords_ReturnsBadRequest(string weakPassword, string reason)
    {
        var response = await Auth.RegisterNewRandomUser(DataGenerator.GetRandomUserName(), DataGenerator.GetRandomEmail(), weakPassword);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(reason);
    }

    [Theory(DisplayName = "Failure: Register with invalid email formats returns BadRequest")]
    [InlineData("plainaddress", "Missing @ and domain")]
    [InlineData("@no-username.com", "Missing username")]
    [InlineData("email@domain..com", "Double dot in domain")]
    [InlineData("invalid-email@", "Missing domain")]
    public async Task Register_InvalidEmail_ReturnsBadRequest(string invalidEmail, string reason)
    {
        var response = await Auth.RegisterNewRandomUser(DataGenerator.GetRandomUserName(), invalidEmail, DataGenerator.GetRandomPassword());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("E-mail is not valid");
    }

    [Fact(DisplayName = "Failure: Registering with an Email that already exists returns 400")]
    public async Task Register_EmailAlreadyExists_ReturnsBadRequest()
    {
        var activeUser = await DbAuth.GetExistingActiveUser();
        activeUser.Should().NotBeNull();

        var response = await Auth.RegisterNewRandomUser(activeUser.UserName, activeUser.Email, DataGenerator.GetRandomPassword());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.ReadAsJsonAsync<ReturnApi<object>>();
        result.Message.Should().Contain("O E-mail já está sendo utilizado.");
    }

    [Theory(DisplayName = "Failure: Registering with invalid UserNames returns BadRequest")]
    [InlineData("ab", "User name must be at least 3 characters.")]
    [InlineData("ThisNameIsWayTooLongForTheDatabase", "User name cannot exceed 20 characters.")]
    [InlineData("User@Name", "User name can only contain letters, numbers, and underscores.")]
    [InlineData("User Name", "User name can only contain letters, numbers, and underscores.")]
    public async Task Register_InvalidUserName_ReturnsBadRequest(string invalidName, string expectedError)
    {
        var response = await Auth.RegisterNewRandomUser(invalidName, DataGenerator.GetRandomEmail(), DataGenerator.GetRandomPassword());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(expectedError);
    }

    [Fact(DisplayName = "Success: Registering with a username that already exists returns 400")]
    public async Task Register_UserAlreadyExists_Returns400()
    {
        var activeUser = await DbAuth.GetExistingActiveUser();
        var response = await Auth.RegisterNewRandomUser(activeUser.UserName, activeUser.Email, DataGenerator.GetRandomPassword());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.ReadAsJsonAsync<ReturnApi<object>>();
        result.Message.Should().Contain("O E-mail já está sendo utilizado.");
    }
    #endregion

    #region Primary Authentication

    [Fact(DisplayName = "Failure: Unsupported grant type returns 400")]
    public async Task Post_InvalidGrantType_ReturnsBadRequest()
    {
        var response = await Auth.GetInitialAuthToken(DataGenerator.GetRandomEmail(), DataGenerator.GetRandomPassword(), "InvalidGrantType");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("not supported");
    }

    [Fact(DisplayName = "Failure: Wrong password return 401")]
    public async Task Post_WrongPassword_ReturnsUnauthorized()
    {
        var activeUser = await DbAuth.GetExistingActiveUser();
        activeUser.Should().NotBeNull();

        var resp = await Auth.GetInitialAuthToken(activeUser.Email, DataGenerator.GetRandomPassword(), DataGenerator.GrantType);

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Failure: Wrong email return 401")]
    public async Task Post_WrongEmail_ReturnsUnauthorized()
    {
        var resp = await Auth.GetInitialAuthToken(DataGenerator.GetRandomEmail(), DataGenerator.GetRandomPassword(), DataGenerator.GrantType);

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Second Authentication

    [Fact(DisplayName = "Happy: Resend second authentication returns OK")]
    public async Task ResendSecondAuth_ReturnsOk()
    {
        var (_, _, secondAuthToken) = await RegisterResetAndGetSecondAuthTokenAsync();

        var resendForm = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("secondAuthToken", secondAuthToken)
        });

        var resendResp = await _client.PostAsync("/user/Authentication/resendSecondAuthentication", resendForm);
        resendResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Failure: Resend second authentication with malformed token returns BadRequest")]
    public async Task ResendSecondAuth_InvalidToken_ReturnsBadRequest()
    {
        var resendForm = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("secondAuthToken", "invalid-token")
        });

        var response = await _client.PostAsync("/user/Authentication/resendSecondAuthentication", resendForm);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.ReadAsJsonAsync<ReturnApi<object>>();
        result.Message.Should().Contain("Invalid token");
    }

    [Fact(DisplayName = "Full Flow: Temporary password login -> Reset -> Login with new password")]
    public async Task FullFlow_TemporaryPassword_Lifecycle_Succeeds()
    {
        var (email, _, secondAuthToken) = await RegisterResetAndGetSecondAuthTokenAsync();
        var code = await WaitForEmailValueAsync(email);
        var bearerToken = await Auth.SendSecondAuthCode(secondAuthToken, code);

        var errorResult = await bearerToken.Content.ReadAsStringAsync();
        errorResult.Should().NotContain("Code Expired");
        bearerToken.IsSuccessStatusCode.Should().BeTrue("Expected successful login after full flow.");
    }

    [Fact(DisplayName = "Failure: Second auth with malformed/invalid token encryption")]
    public async Task SecondAuth_InvalidToken_ReturnsBadRequest()
    {
        var token = "randomencryptedcookie";

        var resp = await Auth.SendSecondAuthCode(token, "12341");

        var result = await resp.Content.ReadFromJsonAsync<ReturnApi<object>>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Contain("Invalid token");
    }

    [Fact(DisplayName = "Failure: Second auth with valid token but expired/missing in cache")]
    public async Task SecondAuth_ExpiredCacheToken_ReturnsBadRequest()
    {
        var fakeToken = Guid.NewGuid().ToString().EncryptCookie();

        var resp = await Auth.SendSecondAuthCode(fakeToken, "12341");

        var result = await resp.Content.ReadFromJsonAsync<ReturnApi<object>>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Contain("Reset token expired");
    }

    [Fact(DisplayName = "Failure: Second auth with correct token but wrong 2FA code")]
    public async Task SecondAuth_WrongCode_ReturnsBadRequest()
    {
        var (_, _, secondAuthToken) = await RegisterResetAndGetSecondAuthTokenAsync();
        var respSecondAuth = await Auth.SendSecondAuthCode(secondAuthToken, "12341");

        respSecondAuth.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorResult = await respSecondAuth.Content.ReadAsStringAsync();
        errorResult.Should().Contain("Code Expired");
    }

    [Fact(DisplayName = "Happy: Second auth with valid code returns bearer token")]
    public async Task SecondAuth_ValidCode_ReturnsBearerToken()
    {
        var (email, _, secondAuthToken) = await RegisterResetAndGetSecondAuthTokenAsync();
        var code = await WaitForEmailValueAsync(email);
        var secondResp = await Auth.SendSecondAuthCode(secondAuthToken, code);
        secondResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondPayload = await secondResp.ReadAsJsonAsync<ReturnApi<BearerToken>>();
        secondPayload?.Data?.AccessToken.Should().NotBeNull();
        secondPayload.Data.TokenType.Should().Be("Bearer");
    }

    #endregion

    #region Password Reset & Logic

    [Fact(DisplayName = "Happy: Forgot password for existing username returns OK")]
    public async Task ForgotPassword_ExistingUser_ReturnsOk()
    {
        var (_, email, _) = await RegisterRandomUser();

        var resp = await _client.PutAsJsonAsync("/user/Authentication/forgotPassword", new { Email = email });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await resp.Content.ReadAsStringAsync();
        result.Should().Contain("Password reset link sent to your email");
    }

    [Fact(DisplayName = "Failure: Reset password with wrong Email")]
    public async Task ResetPassword_WrongEmail_ReturnsBadRequest()
    {      
        var forgotResp = await Auth.ForgotPassword(DataGenerator.GetRandomEmail());
        forgotResp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Failure: Reset password with malformed token returns bad request")]
    public async Task ResetPassword_InvalidToken_ReturnsBadRequest()
    {
        var request = new ResetPasswordRequest
        {
            ResetPwdToken = "invalid-token",
            NewPassword = "ValidPassword123!",
            RepeatPassword = "ValidPassword123!"
        };

        var response = await _client.PutAsJsonAsync("/user/Authentication/resetPassword", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.ReadAsJsonAsync<ReturnApi<object>>();
        result.Message.Should().Contain("Invalid token");
    }

    [Fact(DisplayName = "Failure: Reset password with mismatched repeat password")]
    public async Task ResetPassword_Mismatch_ReturnsBadRequest()
    {
        var (_, email, password) = await RegisterRandomUser();

        await Auth.ForgotPassword(email);
       
        var tempPassword = await WaitForEmailValueAsync(email);

        var resetToken = await Auth.GetResetTokenFromLogin(email, tempPassword);

        var resp = await Auth.ResetPasswordWrongRepeated(resetToken, DataGenerator.GetRandomPassword());
        
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Failure: Reset password with repeated (Older) password")]
    public async Task ResetPassword_OlderPassword_ReturnsBadRequest()
    {        
        var (_, email, originalPassword) = await RegisterRandomUser();

        var forgotResp = await Auth.ForgotPassword(email);
        forgotResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var tempPassword = await WaitForEmailValueAsync(email);
        
        var resetToken = await Auth.GetResetTokenFromLogin(email, tempPassword);
        resetToken.Should().NotBeNull();

        var resp = await Auth.ResetPassword(resetToken, originalPassword);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorContent = await resp.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Password must be different from the last 10");
    }

    [Fact(DisplayName = "Failure: Reset password with Expired Token")]
    public async Task ResetPassword_ExpiredToken_ReturnsBadRequest()
    {
        var (_, email, originalPassword) = await RegisterRandomUser();

        await Auth.ForgotPassword(email);

        var tempPassword = await WaitForEmailValueAsync(email);

        var resetToken = await Auth.GetResetTokenFromLogin(email, tempPassword);

        factory.ExpireTokenInCache(resetToken);
     
        var resp = await Auth.ResetPassword(resetToken, originalPassword);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorContent = await resp.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Reset token expired");        
    }

    [Fact(DisplayName = "Failure: Cannot reuse the same Reset Token twice")]
    public async Task ResetPassword_TokenReuse_ReturnsBadRequest()
    {
        var (_, email, password) = await RegisterRandomUser();
        var newPassword = DataGenerator.GetRandomPassword();
        
        var forgotResp = await Auth.ForgotPassword(email);
        forgotResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var tempPassword = await WaitForEmailValueAsync(email);

        var resetToken = await Auth.GetResetTokenFromLogin(email, tempPassword);
        resetToken.Should().NotBeNull();

        var resp = await Auth.ResetPassword(resetToken, newPassword);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var resp1 = await Auth.ResetPassword(resetToken, DataGenerator.GetRandomPassword());
        resp1.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorContent = await resp1.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Reset token expired");
    }

    [Fact(DisplayName = "Failure: Expired password returns 403")]
    public async Task Post_ExpiredPassword_Returns403()
    {      
        var (_, email, password) = await RegisterRandomUser();

        await DbAuth.SetUserPasswordExpirationAsync(email, DateTime.UtcNow.AddMinutes(-6), isTemporary: true);

        var resp =  await Auth.GetInitialAuthToken(email, password, DataGenerator.GrantType);
        resp.StatusCode.Should().Be((HttpStatusCode)403);

        var errorContent = await resp.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Temporary Password Expired");
    }

    [Fact(DisplayName = "Success: Valid temporary password returns 201 and Reset Token")]
    public async Task Post_ValidTemporaryPassword_Returns201AndToken()
    {
        var (_, email, password) = await RegisterRandomUser();

        var resp = await Auth.GetInitialAuthToken(email, password, DataGenerator.GrantType);
        resp.StatusCode.Should().Be(HttpStatusCode.Created);

        var errorContent = await resp.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Valid temporary password, update password");
    }

    [Fact(DisplayName = "Failure: Regular expired password returns 403 and Reset Token")]
    public async Task Post_RegularPasswordExpired_Returns403AndToken()
    {
        var (_, email, password) = await RegisterRandomUser();

        await DbAuth.SetUserPasswordExpirationAsync(email, DateTime.UtcNow.AddDays(-91), isTemporary: false);

        var resp = await Auth.GetInitialAuthToken(email, password, DataGenerator.GrantType);

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var result = await resp.Content.ReadFromJsonAsync<ReturnApi<LoginReset>>();
        result.Message.Should().Be("Password expired, update it");
        result.Data.ResetPwdToken.Should().NotBeNull();
    }

    #endregion
}