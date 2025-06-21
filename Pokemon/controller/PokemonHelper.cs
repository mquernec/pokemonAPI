public static class PokemonControllerHelper
{
    public static IServiceCollection AddPokemonController(this IServiceCollection services)
    {
        // Register all controllers
        services.AddControllers()
            .AddApplicationPart(typeof(PokemonController).Assembly)
            .AddApplicationPart(typeof(TrainerController).Assembly)
            .AddApplicationPart(typeof(BattleController).Assembly)
            .AddApplicationPart(typeof(MonitoringController).Assembly);

        return services;
    }
}

