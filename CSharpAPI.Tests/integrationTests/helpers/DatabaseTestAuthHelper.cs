using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace CSharpAPI.Tests.integrationTests.helpers
{
    public class DatabaseTestAuthHelper(IConfiguration configuration)
    {
        private readonly IConfiguration _configuration = configuration;

        private IDbConnection CreateConnection()
        {
            var connectionString = _configuration.GetConnectionString("Default")
                ?? throw new Exception("Connection string 'Default' not found.");
            return new SqlConnection(connectionString);
        }
        public async Task<(string Email, string UserName)> GetExistingActiveUser()
        {
            using var conn = CreateConnection();
            var result = await conn.QueryFirstOrDefaultAsync<(string Email, string UserName)>(@"
                SELECT TOP 1 
                    EMAIL, 
                    NAME AS UserName
                FROM USERS
                WHERE ACTIVE = 1 
                ORDER BY COD_USER DESC");

            return result;
        }

        public async Task SetUserPasswordExpirationAsync(string email, DateTime expirationDate, bool isTemporary = false)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(@"
                WITH LatestPassword AS (
                    SELECT TOP 1 up.CREATED_AT, up.TEMPORARY
                    FROM USER_PASSWORD up
                    INNER JOIN USERS u ON up.COD_USER = u.COD_USER
                    WHERE u.EMAIL = @Email AND up.ACTIVE = 1
                    ORDER BY up.CREATED_AT DESC
                )
                UPDATE LatestPassword 
                SET CREATED_AT = @OldDate, 
                    TEMPORARY = @IsTemp;",
                new
                {
                    OldDate = expirationDate,
                    Email = email,
                    IsTemp = isTemporary ? 1 : 0
                });
        }
    }
}