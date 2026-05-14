using Dominion.Dominion.Cards;
using Dominion.Dominion.Game;
using Dominion.Dominion.Game.Debug;
using Dominion.Dominion.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// =====================
// CORE CONTENT SYSTEM
// =====================

// Effect Resolver
builder.Services.AddSingleton<EffectResolver>();

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
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

app.Run();