using System.Data;
using API.Domain.Auth;
using API.Infrastructure.Interface;
using API.Utilities;
using Dapper;
using Microsoft.Data.SqlClient;


namespace API.Infrastructure.Dao
{
    public class AuthDao(IConfiguration configuration) : IAuthDao
    {
        private readonly string _connStr = configuration.GetConnectionString("Default");
        private SqlConnection _connection;

        private SqlConnection Connection => _connection ??= new SqlConnection(_connStr);

        public async Task<AuthResult> AccessApi(AuthRequest request)
        {
            const string proc = "SP_LOGIN_USER";

            var result = await Connection.QueryFirstOrDefaultAsync<AuthResult>(proc,
                new
                {
                    EMAIL = request.Email
                },
                commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<AuthResult> AccessApi(ForgotPassword request)
        {
            const string proc = "SP_LOGIN_USER";

            var result = await Connection.QueryFirstOrDefaultAsync<AuthResult>(proc,
                new
                {
                    EMAIL = request.Email
                },
                commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task ResetPassword(ResetPasswordRequest request, string email)
        {
            const string proc = "SP_REG_USER_PASSWORD";

            request.NewPassword = request.NewPassword.PasswordEncryption();

            await Connection.ExecuteAsync(proc, new
            {
                EMAIL = email,
                PASSWORD = request.NewPassword
            }, commandType: CommandType.StoredProcedure);
        }

        public async Task<List<PassHist>> PasswordHistory(int codUser)
        {
            const string proc = "SP_LS_VALIDATE_PASSWORD";

            var pass = (await Connection.QueryAsync<PassHist>(proc, new
            {
                COD_USER = codUser
            }, commandType: CommandType.StoredProcedure)).AsList();

            return pass;
        }

        public async Task ForgotPassword(ForgotPassword request, string defaultPassword)
        {
            const string proc = "SP_GENERATE_TEMPORARY_PASSWORD_USER";

            await Connection.ExecuteAsync(proc, new
            {
                EMAIL = request.Email,
                TEMPORARY = defaultPassword

            }, commandType: CommandType.StoredProcedure);
        }

        public async Task GenerateSecondAuth(SecondAuthenticationRequest request)
        {
            const string proc = "SP_REGISTER_SECOND_FACTOR_PASS";

            await Connection.ExecuteAsync(proc, new
            {
                COD_USER = request.CodUser.DecryptInt(),
                VALUE = request.Code,
            }, commandType: CommandType.StoredProcedure);
        }

        public async Task<AuthResult> SecondAuthentication(SecondAuthenticationRequest request)
        {
            const string proc = "SP_INITIAL_ACCESS_SECOND_FACTOR";

            return await Connection.QueryFirstOrDefaultAsync<AuthResult>(proc, new
            {
                COD_USER = request.CodUser.DecryptInt(),
                VALUE = request.Code
            }, commandType: CommandType.StoredProcedure);
        }
    }
}