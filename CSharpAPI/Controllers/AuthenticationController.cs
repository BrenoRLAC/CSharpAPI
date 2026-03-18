using API.Domain;
using API.Domain.Auth;
using API.Infrastructure.Interface;
using API.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace API.Controllers
{
    [ApiController]
    [Route("user/[controller]")]
    public class AuthenticationController(IAuthService service, ILogger<AuthenticationController> logger, IMemoryCache cache) : ControllerBase
    {
        private readonly IAuthService _service = service;
        private readonly ILogger<AuthenticationController> _logger = logger;
        private readonly IMemoryCache _cache = cache;

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        [Produces("application/json", Type = typeof(BearerToken))]
        [ProducesResponseType(typeof(ReturnApi<object>), 401)]
        [ProducesResponseType(typeof(ReturnApi<LoginReset>), 201)]
        [ProducesResponseType(typeof(ReturnApi<SecondAuthToken>), 201)]
        public async Task<IActionResult> Post([FromForm] AuthRequest request)
        {
            _logger.LogInformation("[API] POST /Authentication {@Request}", request);

            if (!request.GrantType.Equals("password", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new ReturnApi<object>(400, "This grant type is not supported."));

            var result = await _service.AccessApi(request);

            if (result == null)
                return Unauthorized(new ReturnApi<object>(401, "Invalid Username or password"));

            if (result.PasswordExpired)
            {
                var tmp = Guid.NewGuid().ToString();
                _cache.Set(tmp, JsonSerializer.Serialize(request), TimeSpan.FromMinutes(5));
                return StatusCode(403, new ReturnApi<LoginReset>(403, "Password expired, update it", new LoginReset { ResetPwdToken = tmp.EncryptCookie() }));
            }

            if (result.Temporary)
            {
                if (result.TemporaryPasswordExpired)
                    return StatusCode(403, new ReturnApi<object>(403, "Temporary Password Expired"));

                var tmpTokenPassword = Guid.NewGuid().ToString();
                _cache.Set(tmpTokenPassword, JsonSerializer.Serialize(request), TimeSpan.FromMinutes(5));

                return StatusCode(201, new ReturnApi<LoginResetTemporary>(201, "Valid temporary password, update password", new LoginResetTemporary { data = new LoginReset { ResetPwdToken = tmpTokenPassword.EncryptCookie() } }));
            }

            var secondAuthRequest = new SecondAuthenticationRequest()
            {
                CodUser = result.CodUser.EncryptInt(),
                Email = result.Email,
                Username = result.Username
            };

            var tmpTokenSecondAuth = Guid.NewGuid().ToString();
            _cache.Set(tmpTokenSecondAuth, JsonSerializer.Serialize(secondAuthRequest), TimeSpan.FromMinutes(5));

            try
            {
                await _service.GenerateSecondAuth(secondAuthRequest);

            }
            catch (Exception e)
            {
                return BadRequest(new ReturnApi<object>(400, e.Message));

            }

            return Ok(new ReturnApi<SecondAuthToken>(
                200,
                "Second authentication code sent to your email",
                new SecondAuthToken { SecondAuthenticationToken = tmpTokenSecondAuth.EncryptCookie() }
            ));
        }

        [HttpPut, Route("resetPassword")]
        [Consumes("application/json"), Produces("application/json", Type = typeof(ResetPasswordRequest))]
        [ProducesResponseType(typeof(ReturnApi<object>), 400)]
        [ProducesResponseType(typeof(ReturnApi<object>), 401)]
        [ProducesResponseType(typeof(ReturnApi<object>), 200)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            _logger.LogInformation("Request PUT /resetPassword {@Request}", request);

            if (!request.ResetPwdToken.TryDecryptCookie(out string token))
                return BadRequest(new ReturnApi<object>(400, "Invalid token"));

            if (!_cache.TryGetValue(token, out string authrequest))
                return BadRequest(new ReturnApi<object>(400, "Reset token expired"));

            if (!request.NewPassword.Equals(request.RepeatPassword))
                return BadRequest(new ReturnApi<object>(400, "Both NewPassword and RepeatPassword must be equal"));

            if (!request.NewPassword.IsValidPassword(out string error))
                return BadRequest(new ReturnApi<object>(400, error));

            var auth = JsonSerializer.Deserialize<AuthRequest>(authrequest);

            bool pwdRepeated = await _service.PwdRepeated(auth.Email, request.NewPassword);
            if (pwdRepeated)
                return BadRequest(new ReturnApi<object>(400, "Password must be different from the last 10"));

            await _service.ResetPassword(request, auth.Email);

            _cache.Remove(token);

            return Ok(new ReturnApi<object>(200, "Password updated successfully"));
        }

        [Route("forgotPassword")]
        [HttpPut, Produces("application/json")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPassword request)
        {
            _logger.LogInformation("[API] POST /Authentication/forgotPassword {@Request}", request);

            var result = await _service.ForgotPassword(request);

            if (result == null)
                return BadRequest(new ReturnApi<object>(400, "Invalid Username"));

            return Ok(new ReturnApi<object>(200, "Password reset link sent to your email"));
        }

        [Route("secondAuthentication/{code}")]
        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        [Produces("application/json", Type = typeof(BearerToken))]
        public async Task<IActionResult> SecondAuthentication([FromForm] SecondAuthToken request, [FromRoute] string code)
        {
            _logger.LogInformation("[HTTP] Request POST /Authentication/secondAuthentication/{code} {@Request}", code, request);

            if (!request.SecondAuthenticationToken.TryDecryptCookie(out string tokenSecondAuth))
                return BadRequest(new ReturnApi<object>(400, "Invalid token"));

            if (!_cache.TryGetValue(tokenSecondAuth, out string secondAuthRequest))
                return BadRequest(new ReturnApi<object>(400, "Reset token expired"));

            var secondAuth = JsonSerializer.Deserialize<SecondAuthenticationRequest>(secondAuthRequest);

            secondAuth.Code = code;

            var result = await _service.SecondAuthentication(secondAuth);
            if (result == null)
                return BadRequest(new ReturnApi<object>(400, "Code Expired"));

            var token = _service.Generate(result);

            _cache.Remove(tokenSecondAuth);

            return Ok(new ReturnApi<BearerToken>(200, token));

        }

        [Route("resendSecondAuthentication")]
        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        [Produces("application/json", Type = typeof(object))]
        public async Task<IActionResult> ResendSecondAuthentication([FromForm] SecondAuthToken request)
        {
            _logger.LogInformation("[HTTP] Request POST /Authentication/resendSecondAuthentication {@Request}", request);

            if (!request.SecondAuthenticationToken.TryDecryptCookie(out string tokenSecondAuth))
                return BadRequest(new ReturnApi<object>(400, "Invalid token"));

            if (!_cache.TryGetValue(tokenSecondAuth, out string secondAuthRequest))
                return BadRequest(new ReturnApi<object>(400, "Reset token expired"));

            var secondAuth = JsonSerializer.Deserialize<SecondAuthenticationRequest>(secondAuthRequest);

            await _service.GenerateSecondAuth(secondAuth);

            return Ok(new ReturnApi<OkResult>(200, new OkResult()));

        }

        [HttpPost("register")]
        public async Task<IActionResult> SignUp([FromBody] UserRequest request)
        {
            if (!request.Password.IsValidPassword(out string error))
                return BadRequest(new ReturnApi<object>(400, error));
            try
            {
                await _service.SignUp(request);
                return Ok(new ReturnApi<object>(200, "User Successfully registered!"));
            }
            catch (SqlException ex) when (ex.Number == 50001 || ex.Number == 50002)
            {
                return BadRequest(new ReturnApi<object>(400, ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, new ReturnApi<object>(500, "An unexpected error occurred."));
            }
        }

    }
}