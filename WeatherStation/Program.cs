using WeatherStation.Models;
using WeatherStation.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<SensorDatabaseSettings>(
    builder.Configuration.GetSection("WeatherStationDatabase"));

builder.Services.Configure<RabbitMqConfiguration>(
    builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddSingleton<SensorService>();
builder.Services.AddSingleton<RabbitMqService>();

builder.Services.AddControllers()
    .AddJsonOptions(
        options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

var app = builder.Build();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();