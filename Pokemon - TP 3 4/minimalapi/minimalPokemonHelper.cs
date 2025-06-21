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
            var pokemon = pokemonService.GetPokemonByName(name);
            return pokemon is not null ? Results.Ok(pokemon) : Results.NotFound();
        });

        app.MapPost("/pokemons", ([FromBody] Pokemon pokemon, [FromServices] IPokemonService pokemonService) =>
        {
            if (pokemon is null)
            {
                return Results.BadRequest("Pokemon cannot be null.");
            }

            try
            {
                pokemonService.CreatePokemon(pokemon);
                return Results.Created($"/pokemons/{pokemon.Name}", pokemon);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });
        app.MapPut("/pokemons/{id}", (int id, [FromBody] Pokemon pokemon, [FromServices] IPokemonService pokemonService) =>
        {
            if (pokemon is null || pokemon.Id != id)
            {
                return Results.BadRequest("Pokemon cannot be null and ID must match.");
            }

            try
            {
                pokemonService.UpdatePokemon(pokemon);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });
        app.MapDelete("/pokemons/{id}", (int id, [FromServices] IPokemonService pokemonService) =>
        {
            try
            {
                pokemonService.DeletePokemon(id);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });
        app.MapGet("/pokemons/type/{type}", (string type, [FromServices] IPokemonService pokemonService) =>
        {
            var pokemons = pokemonService.GetPokemonByType(type);
            return Results.Ok(pokemons);
        })
        .WithName("GetPokemonsByType")
        .WithSummary("Get Pokémon by Type")
        .WithDescription("This endpoint returns a list of Pokémon filtered by type.")
        .Produces<IEnumerable<Pokemon>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
        app.MapGet("/pokemons/level/{minLevel}/{maxLevel}", (int minLevel, int maxLevel, [FromServices] IPokemonService pokemonService) =>
        {
            var pokemons = pokemonService.GetPokemonByLevel(minLevel, maxLevel);
            return Results.Ok(pokemons);
        })
        .WithName("GetPokemonsByLevel")
        .WithSummary("Get Pokémon by Level")
        .WithDescription("This endpoint returns a list of Pokémon filtered by level range.")
        .Produces<IEnumerable<Pokemon>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        
        app.MapGet("/", () => "Welcome to the Pokémon API! Use /pokemons to get a list of Pokémon.")
            .WithName("GetRoot")
            .WithSummary("Root Endpoint")
            .WithDescription("This endpoint returns a welcome message for the Pokémon API root.");
    }
}