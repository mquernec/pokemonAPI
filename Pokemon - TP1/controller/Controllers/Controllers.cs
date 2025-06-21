using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


/*
[Route("api/[controller]")]
[ApiController]
public class PokemonController : ControllerBase
{
    public IEnumerable<string> GetAllPokemon()
    {
        return new List<string> { "Pikachu", "Bulbasaur", "Charmander" };
    }
}
*/
/*

    [Route("api/[controller]")]
    [ApiController]
    public class PokemonController : ControllerBase
    {

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        [ApiExplorerSettings(GroupName = "v1")]
        [HttpGet]
        [Route("/pokemons")]
        public IEnumerable<string> GetAllPokemons()
        {
            // Liste complète des noms de Pokémon de tous les jeux principaux (Générations 1 à 9)
            return new List<string>
            {
                "Bulbasaur", "Ivysaur", "Venusaur", "Charmander", "Charmeleon", "Charizard", "Squirtle", "Wartortle", "Blastoise",
                "Caterpie", "Metapod", "Butterfree", "Weedle", "Kakuna", "Beedrill", "Pidgey", "Pidgeotto", "Pidgeot",
                "Rattata", "Raticate", "Spearow", "Fearow", "Ekans", "Arbok", "Pikachu", "Raichu", "Sandshrew", "Sandslash",
                "Nidoran♀", "Nidorina", "Nidoqueen", "Nidoran♂", "Nidorino", "Nidoking", "Clefairy", "Clefable", "Vulpix", "Ninetales",
                "Jigglypuff", "Wigglytuff", "Zubat", "Golbat", "Oddish", "Gloom", "Vileplume", "Paras", "Parasect", "Venonat", "Venomoth",
                "Diglett", "Dugtrio", "Meowth", "Persian", "Psyduck", "Golduck", "Mankey", "Primeape", "Growlithe", "Arcanine",
                "Poliwag", "Poliwhirl", "Poliwrath", "Abra", "Kadabra", "Alakazam", "Machop", "Machoke", "Machamp", "Bellsprout",
                "Weepinbell", "Victreebel", "Tentacool", "Tentacruel", "Geodude", "Graveler", "Golem", "Ponyta", "Rapidash", "Slowpoke",
                "Slowbro", "Magnemite", "Magneton", "Farfetch’d", "Doduo", "Dodrio", "Seel", "Dewgong", "Grimer", "Muk", "Shellder",
                "Cloyster", "Gastly", "Haunter", "Gengar", "Onix", "Drowzee", "Hypno", "Krabby", "Kingler", "Voltorb", "Electrode",
                "Exeggcute", "Exeggutor", "Cubone", "Marowak", "Hitmonlee", "Hitmonchan", "Lickitung", "Koffing", "Weezing", "Rhyhorn",
                "Rhydon", "Chansey", "Tangela", "Kangaskhan", "Horsea", "Seadra", "Goldeen", "Seaking", "Staryu", "Starmie",
                "Mr. Mime", "Scyther", "Jynx", "Electabuzz", "Magmar", "Pinsir", "Tauros", "Magikarp", "Gyarados", "Lapras", "Ditto",
                "Eevee", "Vaporeon", "Jolteon", "Flareon", "Porygon", "Omanyte", "Omastar", "Kabuto", "Kabutops", "Aerodactyl",
                "Snorlax", "Articuno", "Zapdos", "Moltres", "Dratini", "Dragonair", "Dragonite", "Mewtwo", "Mew",
                // Générations suivantes (exemple, à compléter avec tous les noms jusqu'à la génération 9)
                "Chikorita", "Bayleef", "Meganium", "Cyndaquil", "Quilava", "Typhlosion", "Totodile", "Croconaw", "Feraligatr",
                // ... (ajoutez ici tous les autres Pokémon jusqu'à la génération 9)
                "Sprigatito", "Floragato", "Meowscarada", "Fuecoco", "Crocalor", "Skeledirge", "Quaxly", "Quaxwell", "Quaquaval"
                // Pour la liste complète, utilisez une source officielle ou une base de données Pokémon.
            };
        }



        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        [ApiExplorerSettings(GroupName = "v1")]
        [HttpGet]
        [Route("/pokemons/{id}")]
        public string GetPokemon(int id)
        {
            var pokemons = new List<string> { "Pikachu", "Bulbasaur", "Charmander" };
            return pokemons.ElementAtOrDefault(id) ?? "Pokemon not found";
        }
    }*/

  [Route("api/[controller]")]
  [ApiController]
   public class PokemonController : ControllerBase
    {

    private readonly IPokemonService _pokemonService;

    public PokemonController(IPokemonService pokemonService)
    {
        _pokemonService = pokemonService;
    }
    [HttpGet("pokemons2")]
    public async Task<Result<Ok<IEnumerable<string>>, ErrorResponse>> GetAllPokemons2()
    {
        try
        {
            var pokemons = await _pokemonService.GetAllPokemonAsync();
            return Ok(pokemons);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
    [HttpGet("pokemons")]
    public ActionResult<IEnumerable<string>> GetAllPokemons()
    {
        try
        {
            var pokemons = _pokemonService.GetAllPokemon();
            return Ok(pokemons);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpGet("pokemons/{id}")]
    public ActionResult<string> GetPokemon(string id)
    {
        try
        {
            var pokemon = _pokemonService.GetPokemonByName(id);
            if (pokemon == null)
            {
                return NotFound();
            }
            return Ok(pokemon);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}

