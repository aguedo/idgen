using Aslanta.Idgen.Api;

var builder = WebApplication.CreateBuilder(args);

Config.ConnectionString = GetConnectionString(builder.Configuration);

builder.Services.AddSingleton<ICacheRepository, CacheRepository>();
builder.Services.AddSingleton<IdCache>();

var app = builder.Build();

app.MapGet("/", async (IdCache cache) => new
{
    Id = await cache.GetId().ConfigureAwait(false)
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
