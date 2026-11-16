using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Games;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Games.KingdomHearts.Unreal;

namespace NexusMods.Games.KingdomHearts;

public static class Services
{
    public static IServiceCollection AddKingdomHeartsGames(this IServiceCollection services)
    {
        services
            .AddGame<KH3>()
            .AddSingleton<ITool, RunGameTool<KH3>>()
            .AddGame<KH02>()
            .AddSingleton<ITool, RunGameTool<KH02>>();

        return services;
    }
}
