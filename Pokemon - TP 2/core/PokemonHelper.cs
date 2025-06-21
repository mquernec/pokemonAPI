using Microsoft.Extensions.DependencyInjection;

public static class PokemonHelper
{
    public static IServiceCollection AddPokemonServices(this IServiceCollection services)
    {
        // Register Pokemon services
        services.AddSingleton<IPokemonRepository, PokemonRepository>();
        services.AddSingleton<IPokemonService, PokemonService>();

        // Register Trainer services
        services.AddSingleton<ITrainerRepository, TrainerRepository>();
        services.AddSingleton<ITrainerService, TrainerService>();

        // Register Battle services
        services.AddSingleton<IBattleRepository, BattleRepository>();
        services.AddSingleton<IBattleService, BattleService>();

        return services;
    }
}
