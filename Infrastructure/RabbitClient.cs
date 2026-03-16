using API.Domain.Notification;
using API.Infrastructure.Interface;
using RabbitMQ.Client;
using System.Text;
using API.Utilities;

namespace API.Infrastructure
{
    public class RabbitClient: IRabbitClient
    {

        private readonly IConnectionFactory _rabbitFactory;
        private IConnection _connection = null;

        public RabbitClient(IConnectionFactory rabbitFactory)
        {
            _rabbitFactory = rabbitFactory;
        }

        private IConnection Connection()
        {
            if (_connection != null) return _connection;

            _connection = _rabbitFactory.CreateConnection();

            using var channel = _connection.CreateModel();

            channel.QueueDeclare("NotificationProcess", false, false, false, null);           

            return _connection;
            ;
        }

        public void SendMessage(NotificationRequest request)
        {
            
            using var channel = Connection().CreateModel();
            channel.QueueDeclare(queue: "NotificationProcess", durable: false, exclusive: false, autoDelete: false, arguments: null);
            var body = Encoding.UTF8.GetBytes(request.ToJson());

            channel.BasicPublish(exchange: "", routingKey: "NotificationProcess", basicProperties: null, body: body);
            channel.QueueDeclare(queue: "NotificationProcess", durable: false, exclusive: false, autoDelete: false, arguments: null);

        }
   
    }
}