
using Aslanta.Idgen.Job;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
if (environment == "Development")
{
    builder.AddJsonFile($"appsettings.development.json", optional: true, reloadOnChange: true);
}

IConfiguration configuration = builder.Build();
Config.ConnectionString = GetConnectionString(configuration);

ShortIdService.GenerateId();
Console.WriteLine("Id generation completed.");

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
