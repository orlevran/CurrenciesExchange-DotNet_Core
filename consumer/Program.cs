using MongoDB.Driver;
using Microsoft.Extensions.Options;
using consumer.Configurations;
using consumer.Services;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Bind Mongo settings
builder.Services.Configure<DataBaseSettings>(builder.Configuration.GetSection("MongoDBSettings"));

// Register Mongo client + database
builder.Services.AddSingleton<IMongoClient>(c =>
{
    var settings = c.GetRequiredService<IOptions<DataBaseSettings>>().Value;

    // More graceful exception handling
    if (string.IsNullOrEmpty(settings.ConnectionString))
    {
        throw new ArgumentException("MongoDB ConnectionString is not configured properly.");
    }
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(db =>
{
    var dbConfig = db.GetRequiredService<IOptions<DataBaseSettings>>().Value;

    if (string.IsNullOrWhiteSpace(dbConfig.DatabaseName))
        throw new ArgumentException("MongoDB DatabaseName is not configured properly.");

    var client = db.GetRequiredService<IMongoClient>();
    return client.GetDatabase(dbConfig.DatabaseName);
});

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IConsumerService, ConsumerService>();
builder.Services.AddSingleton<ILastPackageCache, LastPackageCache>();

builder.Services.Configure<KafkaSettings>(config.GetSection("Kafka"));
if (string.Equals(config["RunMode"], "Kafka", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddHostedService<KafkaConsumerService>();
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.Use(async (ctx, next) =>
{
    var mode = (config["RunMode"] ?? "Kafka").ToLowerInvariant();

    if (mode != "kafka" && ctx.Request.Path.StartsWithSegments("/kafka"))
    {
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        await ctx.Response.WriteAsync("Kafka endpoints disabled (RunMode != Kafka).");
        return;
    }

    await next();
});

app.MapControllers();

app.MapGet("/ping", () =>
{
    return "pong";
});

app.Run();