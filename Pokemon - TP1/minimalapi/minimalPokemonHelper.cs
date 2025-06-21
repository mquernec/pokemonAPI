using Microsoft.AspNetCore.Mvc;

public static class MinimalPokemonHelper
{
    public static void MapPokemonEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/pokemons", ([FromServices] IPokemonService pokemonService) =>
        {
            var result = pokemonService.GetAllPokemon();
            return Results.Ok(result);
        })
        .Produces<IEnumerable<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithSummary("Get all Pokémon")
        .WithDescription("This endpoint returns a list of all Pokémon.")
        .WithName("GetPokemons");
        app.MapGet("/pokemons/{name}", (string name, [FromServices] IPokemonService pokemonService) =>
        {
            var pokemon = pokemonService.GetAllPokemon().FirstOrDefault(p => p.Equals(name, StringComparison.OrdinalIgnoreCase));
            return pokemon is not null ? Results.Ok(pokemon) : Results.NotFound();
        });

        app.MapGet("/", () => "Welcome to the Pokémon API! Use /pokemons to get a list of Pokémon.")
            .WithName("GetRoot")
            .WithSummary("Root Endpoint")
            .WithDescription("This endpoint returns a welcome message for the Pokémon API root.");
    }
}