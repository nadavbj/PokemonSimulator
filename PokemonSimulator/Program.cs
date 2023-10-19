using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using PokemonSimulator.logic;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Pokemon Simulator API", Description = "Simulating pokemon fights", Version = "v1" });
});

builder.Services.AddControllers();

builder.Services.AddSingleton<IFightsDB>(new FightSqlDb(args[0]));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pokemon Simulator API V1");
});
app.MapControllers();

app.Run();