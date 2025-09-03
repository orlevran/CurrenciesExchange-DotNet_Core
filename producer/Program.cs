using MongoDB.Driver;
using producer.Services.ExchangeProviders;
using producer.Configurations;
using Microsoft.Extensions.Options;
using producer.Services;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Bind Mongo settings
builder.Services.Configure<DataBaseSettings>(builder.Configuration.GetSection("MongoDBSettings"));
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddSingleton<IKafkaStreamManager, KafkaStreamManager>();
builder.Services.AddSingleton(new InstanceStamp(Guid.NewGuid().ToString("N"), Environment.ProcessId));

var runMode = config["RunMode"] ?? "Db";

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

builder.Services.AddSingleton<IExchangeFactory>(f =>
{
    var configurations = f.GetRequiredService<IConfiguration>();

    var prodivers = configurations.GetSection("Exchange").Get<List<ExchangeProviderConfig>>() ?? new();

    var active = prodivers.FirstOrDefault(x => x.Active) ??
        throw new NotSupportedException("Exchange: All providers inactive.");

    //var name = active?.Provider?.Trim().ToLowerInvariant() ?? "";
    var providerName = (active.Provider ?? string.Empty).Trim();

    if (string.IsNullOrEmpty(providerName))
    {
        throw new ArgumentException("Exchange: Active provider has empty 'Provider' field.");
    }
    
    var key = active.Key ?? string.Empty;

    switch (providerName.ToLowerInvariant())
    {
        case "currencylayer": return new CurrencyLayerExchangeFactory(key);
        case "exchangerate-api": return new ExchangeRateAPIFactory(key);
        case "fxratesapi": return new FXRateAPIFactory(key);
        default : throw new NotSupportedException($"Exchange: Unknown provider '{providerName}'.");
    }
});

builder.Services.AddSingleton(s =>
{
    var config = s.GetRequiredService<IConfiguration>();
    var providers = config.GetSection("Exchange").Get<List<ExchangeProviderConfig>>() ?? new();
    var active = providers.FirstOrDefault(p => p.Active) ??
        throw new NotSupportedException("Exchange: No active provider.");
    return new ActiveExchangeOptions { Provider = active.Provider, Key = active.Key };
});

builder.Services.AddHttpClient();

builder.Services.AddSingleton<IExchangeClient>(c =>
{
    var httpFactory = c.GetRequiredService<IHttpClientFactory>();
    var factory = c.GetRequiredService<IExchangeFactory>();
    var active = c.GetRequiredService<ActiveExchangeOptions>();

    var http = httpFactory.CreateClient(active.Provider); // no hard-coded name
    return new HttpExchangeClient(
        http,
        factory.UrlBuilder(),
        factory.RequestBuilder(),
        factory.ResponseBuilder());
});

builder.Services.AddSingleton<IExchangeRateService, ExchangeRateService>();
builder.Services.AddSingleton<IKafkaStreamManager, KafkaStreamManager>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.Use(async (ctx, next) =>
{
    var mode = runMode.ToLowerInvariant();

    // block /api/* if Kafka mode
    if (mode == "kafka" && ctx.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
    {
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        await ctx.Response.WriteAsync("DBFetchController disabled (RunMode=Kafka).");
        return;
    }

    // block /kafka/* if Db mode
    if (mode == "db" && ctx.Request.Path.StartsWithSegments("/kafka", StringComparison.OrdinalIgnoreCase))
    {
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        await ctx.Response.WriteAsync("KafkaController disabled (RunMode=Db).");
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

public record InstanceStamp(string Id, int Pid);