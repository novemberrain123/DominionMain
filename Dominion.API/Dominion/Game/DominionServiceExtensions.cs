using Dominion.API.Dominion.Cards;
using Dominion.API.Dominion.Persistance;
using Dominion.API.Dominion.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dominion.API.Dominion.Game;

public static class DominionServiceExtensions
{
    public static IServiceCollection AddDominionServices(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        // Stateless application/domain services
        services.AddSingleton<EffectResolver>();
        services.AddSingleton<ChoiceResolver>();
        services.AddSingleton<ContentLoader>();
        services.AddSingleton<GameModeLoader>();
        services.AddSingleton<SupplyBuilder>();
        services.AddSingleton<GameSetupService>();
        services.AddSingleton<GameStateSerializer>();
        services.AddSingleton<CardDefinitionFactory>();

        // Holds active in-memory games
        services.AddSingleton<GameSessionManager>();

        // Database
        if (configuration is not null)
        {
            services.AddDbContext<DominionDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString(
                        "DominionDatabase")));

            services.AddScoped<GameRepository>();
        }

        // Orchestration
        services.AddTransient<GameEngineFactory>();

        return services;
    }
}