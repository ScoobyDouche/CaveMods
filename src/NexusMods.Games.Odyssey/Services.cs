using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Games;
using NexusMods.Games.Odyssey.KotOR;

namespace NexusMods.Games.Odyssey;

public static class Services
{
    public static IServiceCollection AddOdysseyGames(this IServiceCollection services)
    {
        services.AddGame<KotOR1>();
        return services;
    }
}
