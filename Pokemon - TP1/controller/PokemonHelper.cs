public static class PokemonControllerHelper
{
    public static IServiceCollection AddPokemonController(this IServiceCollection services)
    {

        // Register the PokemonController
        services.AddControllers().AddApplicationPart(typeof(PokemonController).Assembly);

        return services;
    }
}

