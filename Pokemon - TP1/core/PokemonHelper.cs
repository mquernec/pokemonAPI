using Microsoft.Extensions.DependencyInjection;
public static class PokemonHelper
{
    public static IServiceCollection AddPokemonServices(this IServiceCollection services)
    {
        // Register the PokemonService as a singleton
        services.AddSingleton<IPokemonService, PokemonService>();

        // Register the PokemonRepository as a singleton
        services.AddSingleton<IPokemonRepository, PokemonRepository>();



        return services;
    }
}
