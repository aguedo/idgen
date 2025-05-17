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

var app = builder.Build();

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
