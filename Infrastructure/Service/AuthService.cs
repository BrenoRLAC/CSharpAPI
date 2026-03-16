using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Domain.Auth;
using API.Domain.Jobs;
using API.Infrastructure.Interface;
using API.Jobs.Interfaces;
using API.Utilities;
using Hangfire;
using Microsoft.IdentityModel.Tokens;

namespace API.Infrastructure.Service
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthDao _dao;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IAuthDao dao, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _dao = dao;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResult> AccessApi(AuthRequest request)
        {
            var auth = await _dao.AccessApi(request);
            _logger.LogDebug("DB Response {@Auth}", auth);

            if (auth == null) return null;

            if (auth.PasswordExpired) return auth;

            bool pass = request.Password.PasswordValidation(auth.Password);

            return pass ? auth : null;
        }

        public BearerToken Generate(AuthResult result)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var expireToken = int.TryParse(_configuration["ExpireToken"], out var exp) ? exp : 1;
            var key = Encoding.UTF8.GetBytes(_configuration["SigningKey"]);

            var claims = new List<Claim>
        {
            new ("CodUser", result.CodUser.ToString(), ClaimValueTypes.Integer32),
            new (ClaimTypes.Name, result.Username),
            new (ClaimTypes.Email, result.Email)
        };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(expireToken),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Issuer"]
            };

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(securityToken);

            return new BearerToken
            {
                AccessToken = accessToken,
                Created = securityToken.ValidFrom,
                ExpireIn = securityToken.ValidTo,
                TokenType = "Bearer",
                UserName = result.Username
            };
        }

        public Task ResetPassword(ResetPasswordRequest request, string email)
        {
            return _dao.ResetPassword(request, email);
        }


        public async Task<bool> PwdRepeated(int CodUser, string password)
        {
            var passHist = await _dao.PasswordHistory(CodUser);

            foreach (var p in passHist)
            {
                if (password.PasswordValidation(p.Password))
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> PwdRepeated(string email, string pwdEncrypted)
        {
            var userInfo = await _dao.AccessApi(new AuthRequest { Email = email });

            return await PwdRepeated(userInfo.CodUser, pwdEncrypted);
        }

        public async Task<AuthResult> ForgotPassword(ForgotPassword request)
        {
            var access = await _dao.AccessApi(request);

            if (access == null) return null;

            var defaultPassword = AssistantHelpers.GenerateRandomCodeAlphanumeric(12);

            await _dao.ForgotPassword(request, defaultPassword.PasswordEncryption());

            BackgroundJob.Enqueue<ISendEmail>(x => x.Send(null, new ForgotPasswordEmail
            {
                Email = access.Email,
                Name = access.Username,
                TempPassword = defaultPassword,

            }));

            return access;
        }

        public async Task GenerateSecondAuth(SecondAuthenticationRequest request)
        {
            string code = AssistantHelpers.GenerateRandomCodeNumeric();

            request.Code = code;

            await _dao.GenerateSecondAuth(request);

            BackgroundJob.Enqueue<ISendEmail>(x => x.Send(null, new SecondAuthenticationEmail
            {
                Email = request.Email,
                Name = request.Username,
                Code = code
            }));

        }

        public async Task<AuthResult> SecondAuthentication(SecondAuthenticationRequest request)
        {
            var token = await _dao.SecondAuthentication(request);

            return token;
        }

    }
}