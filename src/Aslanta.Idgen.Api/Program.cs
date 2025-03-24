
using Aslanta.Idgen.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IdCache>();
builder.Services.AddSingleton<IIdgenService, IdgenService>();

var app = builder.Build();

app.MapGet("/", async (IIdgenService service) => new
{
    Id = await service.GetId().ConfigureAwait(false)
});

app.Run();
