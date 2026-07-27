using Dominion.API.Dominion.Cards;
using Dominion.API.Dominion.Game;
using Dominion.API.Dominion.Serialization;
using Dominion.API.Hubs;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(
                        new JsonStringEnumConverter(
                            JsonNamingPolicy.CamelCase));
                });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// =====================
// CORE CONTENT SYSTEM
// =====================

// Effect Resolver
builder.Services.AddSingleton<EffectResolver>();

// Choice Resolver
builder.Services.AddSingleton<ChoiceResolver>();

// Card creation pipeline
builder.Services.AddSingleton<CardDefinitionFactory>();

// Registry (runtime card storage)
builder.Services.AddSingleton<CardRegistry>();

// Content loading (cards JSON → registry)
builder.Services.AddSingleton<ContentLoader>();

// Mode loading (mode JSON → GameConfig)
builder.Services.AddSingleton<GameModeLoader>();

// Supply building (config → runtime supply)
builder.Services.AddSingleton<SupplyBuilder>();

// Game setup (initializes players based on config)
builder.Services.AddSingleton<GameSetupService>();

// Engine factory (orchestrates everything)
builder.Services.AddTransient<GameEngineFactory>();
//test
builder.Services.AddSingleton<GameEngineProvider>();

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:1044") 
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// =====================
// APP PIPELINE
// =====================

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();
app.MapHub<GameHub>("/hubs/game");

app.Run();