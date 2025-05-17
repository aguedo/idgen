using System.Threading.RateLimiting;
using Aslanta.Idgen.Api;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

Config.ConnectionString = GetConnectionString(builder.Configuration);

builder.Services.AddSingleton<ICacheRepository, CacheRepository>();
builder.Services.AddSingleton<IdCache>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        string clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: clientIp,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 200,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, ct) =>
    {
        context.HttpContext.Response.Headers.RetryAfter = "60";
        await Task.CompletedTask;
    };
});

var app = builder.Build();

app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseCors();

app.MapGet("/", async (IdCache cache) =>
{
    return new
    {
        Id = await cache.GetId().ConfigureAwait(false)
    };
});

app.MapGet("/error", (HttpContext context, ILogger<Program> logger) =>
{
    IExceptionHandlerFeature? exception = context.Features.Get<IExceptionHandlerFeature>();

    if (exception != null)
    {
        Exception error = exception.Error;
        logger.LogError(error, "An unhandled exception occurred: {Message}", error.Message);
    }

    context.Response.StatusCode = 500;
    return Results.Problem("An error occurred while processing your request.");
});

app.Run();

static string GetConnectionString(IConfiguration config)
{
    var postgresPassword = config["IDGEN_POSTGRES_PASSWORD"];
    var dbSettings = config.GetSection("Database");
    var host = dbSettings["Host"];
    var port = dbSettings["Port"];
    var dbName = dbSettings["Name"];
    var username = dbSettings["Username"];
    return $"Host={host};Port={port};Database={dbName};Username={username};Password={postgresPassword};";
}
