using API.Domain;
using API.Domain.Notification;
using API.Infrastructure.Interface;
using API.Utilities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace API.Infrastructure
{
    public class RabbitListener : BackgroundService
    {
        private readonly ILogger<RabbitListener> _logger;
        private readonly IConnectionFactory _client;
        private IModel _channel;
        private IConnection _connection;
        private readonly INotificationHub _hub;

        public RabbitListener(ILogger<RabbitListener> logger, IConfiguration configuration, INotificationHub hub)
        {
            _logger = logger;
            _client = new ConnectionFactory
            {
                HostName = configuration.GetValue<string>("RabbitMQ:host"),
                Port = configuration.GetValue<int>("RabbitMQ:port"),
                UserName = configuration.GetValue<string>("RabbitMQ:user"),
                Password = configuration.GetValue<string>("RabbitMQ:password"),
                ClientProvidedName = "API.Hero"
            };

            _hub = hub;

        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _connection = _client.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "NotificationProcess",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);


            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Aguardando mensagens...");

            var consumer = new EventingBasicConsumer(_channel);

            NotifyReceiver(consumer);

            await Task.CompletedTask;
        }

        private void NotifyReceiver(EventingBasicConsumer consumer)
        {
            consumer.Received += (_, ea) =>
            {
                var body = ea.Body;
                var json = Encoding.UTF8.GetString(body.ToArray());

                _logger.LogInformation("Mensagem recebida: {Message}", json);
                var message = JsonSerializer.Deserialize<NotificationRequest>(json);

                message?.ReturnUsers.ForEach(user =>
                {
                    _hub.SendNotification(user.Item2,
                        new NotificationData
                        {
                            Category = Enum.TryParse(message.ClassStyle,
                                out CategoryMessage category)
                                ? category
                                : CategoryMessage.error,
                            ContentMessage = message.ContentMessage,                            
                            Title = message.Title,
                            MessageId = user.Item1.DecryptInt(),
                        });
                });
            };
           
            _channel.BasicConsume(queue: "NotificationProcess",
                autoAck: true,
                consumer: consumer);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            _connection.Close();
        }
    }
}
