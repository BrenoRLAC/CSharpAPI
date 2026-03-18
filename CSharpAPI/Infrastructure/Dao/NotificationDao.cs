
using API.Domain;
using API.Domain.Notification;
using API.Infrastructure.Interfaces;
using API.Utilities;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;  

namespace API.Infrastructure.Dao
{
    public class NotificationDao(IConfiguration config) : INotificationDao
    {
        private readonly string _connectStr = config.GetConnectionString("Default");
        private SqlConnection _connection;
        private SqlConnection Connection => _connection ??= new SqlConnection(_connectStr);
        private readonly int _timeout = config.GetValue<int>("Database:CommandTimeout", 300);

        public async Task<List<NotificationData>> ListPendingNotification(int codUser)
        {
            const string proc = "SP_LS_MESSAGE";

            var result = await Connection.QueryAsync<NotificationData>(proc, new
            {   
                COD_USER = codUser
            }, commandType: CommandType.StoredProcedure, commandTimeout: _timeout);

            return result.AsList();
        }


        public async Task UpdateNotification(int messageId, int codUser, bool read)
        {
            const string proc = "SP_UP_MESSAGE";

            await Connection.ExecuteAsync(proc, new
            {
                COD_MESSAGING = messageId,
                COD_USER = codUser,
                READ = read
            }, commandType: CommandType.StoredProcedure, commandTimeout: _timeout);
        }

        public async Task ReadAllNotifications(int codUser)
        {
            const string proc = "SP_UP_MESSAGE_ALL";

            await Connection.ExecuteAsync(proc, new
            {
                COD_USER = codUser,
            }, commandType: CommandType.StoredProcedure, commandTimeout: _timeout);
        }

        public async Task<int> HasUnreadNotification(int codUser)
        {
            const string proc = "SP_HAS_UNREAD_NOTIFICATION";

            return await Connection.QueryFirstAsync<int>(proc, new
            {
                COD_USER = codUser,
            }, commandType: CommandType.StoredProcedure, commandTimeout: _timeout);
        }

        public async Task<NotificationData> SetNotification(string codUser, NotificationDataRequest request)
        {

            const string proc = "SP_PUT_MISSION_NOTIFICATION";

            return await Connection.QueryFirstAsync<NotificationData>(proc, new
            {
                TITLE = request.Title,
                CONTENT_MESSAGE = request.ContentMessage,
                CATEGORY_ID = (int)request.Category,
                COD_USER = codUser.DecryptInt()
            }, commandType: CommandType.StoredProcedure, commandTimeout: _timeout);


        }

    }
}