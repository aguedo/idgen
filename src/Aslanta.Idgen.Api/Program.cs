var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IdCache>();

var app = builder.Build();

app.MapGet("/", async (IdCache cache) => new
{
    Id = await cache.GetId().ConfigureAwait(false)
});

app.Run();
