using API.Domain.Notification;
using API.Infrastructure;
using API.Infrastructure.Dao;
using API.Infrastructure.Interface;
using API.Infrastructure.Service;
using API.Jobs;
using API.Middleware;
using API.Utilities;
using CloudinaryServiceInterface.Infrastructure;
using CloudinaryServices.Infrastructure;
using Hangfire;
using RabbitMQ.Client;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();


var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policyBuilder =>
    {
        policyBuilder.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddHttpClient();
builder.Services.AddTransient<IHeroDao, HeroDao>();
builder.Services.AddTransient<IHeroService, HeroService>();
builder.Services.AddTransient<IAddressService, AddressService>();
builder.Services.AddTransient<IRedisUpdate, RedisUpdate>();
builder.Services.AddTransient<IRedisDao, RedisDao>();
builder.Services.AddTransient<INotificationHub, NotificationHub>();
builder.Services.AddSignalR();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddHangfire(x => x.UseSqlServerStorage(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddHangfireServer();

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var rabbitFactory = new ConnectionFactory
{
    HostName = config["RabbitMQ:host"],
    Port = int.Parse(config["RabbitMQ:port"]),
    UserName = config["RabbitMQ:user"],
    Password = config["RabbitMQ:password"]
};


var rabbitClient = new RabbitClient(rabbitFactory);

builder.Services.AddSingleton<IRabbitClient>(rabbitClient);
builder.Services.AddSingleton<ICloudinaryService, CloudinaryService>();
builder.Services.AddHostedService<RabbitListener>();



builder.Services.AddSingleton<ConnectionMultiplexer>(provider =>
{
    var redisConnection = builder.Configuration["Redis:connection"];
    var configuration = ConfigurationOptions.Parse(redisConnection);
    return ConnectionMultiplexer.Connect(configuration);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHub<NotificationHub>("/notification");

var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();

recurringJobManager.AddOrUpdate<IRedisUpdate>("tempHeroes", x => x.Run(null), cronExpression: builder.Configuration["Intervals:IHeroesUpdate"]);

var notif = new NotificationRequest()
{
    ReturnUsers = new List<(string, string)>()
    {
       (34.EncryptInt(), "Usuario")
    },

};

recurringJobManager.AddOrUpdate<IRabbitClient>("rabbit", x => x.SendMessage(notif), cronExpression: builder.Configuration["Intervals:IHeroNotification"]);

app.UseCors("CorsPolicy");

app.UseHangfireDashboard("/hangfire");
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseMiddleware();
app.MapControllers();
app.Run();
