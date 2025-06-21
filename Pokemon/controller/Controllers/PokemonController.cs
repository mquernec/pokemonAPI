using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Toutes les actions nécessitent une authentification
[ServiceFilter(typeof(ActionStatisticsFilter))] // Collecte des statistiques pour toutes les actions
public class PokemonController : ControllerBase
{
    private readonly IPokemonService _pokemonService;

    public PokemonController(IPokemonService pokemonService)
    {
        _pokemonService = pokemonService;
    }

    /// <summary>
    /// Récupère tous les Pokémon
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IEnumerable<Pokemon>> GetAllPokemons()
    {
        try
        {
            var pokemons = _pokemonService.GetAllPokemon();
            return Ok(pokemons);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Récupère un Pokémon par son ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ExecutionTime] // Trace les temps d'exécution pour cette action critique
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<Pokemon> GetPokemonById(int id)
    {
        try
        {
            var pokemon = _pokemonService.GetPokemonById(id);
            if (pokemon == null)
            {
                return NotFound(new { message = $"Pokémon avec l'ID {id} introuvable." });
            }
            return Ok(pokemon);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Récupère un Pokémon par son nom
    /// </summary>
    [HttpGet("name/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<Pokemon> GetPokemonByName(string name)
    {
        try
        {
            var pokemon = _pokemonService.GetPokemonByName(name);
            if (pokemon == null)
            {
                return NotFound(new { message = $"Pokémon '{name}' introuvable." });
            }
            return Ok(pokemon);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }    /// <summary>
    /// Crée un nouveau Pokémon
    /// </summary>
    [HttpPost]
    [MonitorPerformance(ActionName = "CreatePokemon")] // Opération de création importante
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<Pokemon> CreatePokemon([FromBody] CreatePokemonRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pokemon = new Pokemon(0, request.Name, request.Type, request.Level, request.Ability);
            _pokemonService.CreatePokemon(pokemon);

            return CreatedAtAction(nameof(GetPokemonById), new { id = pokemon.Id }, pokemon);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Met à jour un Pokémon existant
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<Pokemon> UpdatePokemon(int id, [FromBody] UpdatePokemonRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pokemon = new Pokemon(id, request.Name, request.Type, request.Level, request.Ability);
            _pokemonService.UpdatePokemon(pokemon);

            return Ok(pokemon);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Supprime un Pokémon
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult DeletePokemon(int id)
    {
        try
        {
            _pokemonService.DeletePokemon(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Récupère les Pokémon par type
    /// </summary>
    [HttpGet("type/{type}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IEnumerable<Pokemon>> GetPokemonByType(string type)
    {
        try
        {
            var pokemons = _pokemonService.GetPokemonByType(type);
            return Ok(pokemons);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Récupère les Pokémon par plage de niveau
    /// </summary>
    [HttpGet("level")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IEnumerable<Pokemon>> GetPokemonByLevel([FromQuery] int minLevel = 1, [FromQuery] int maxLevel = 100)
    {
        try
        {
            var pokemons = _pokemonService.GetPokemonByLevel(minLevel, maxLevel);
            return Ok(pokemons);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Récupère les Pokémon par capacité
    /// </summary>
    [HttpGet("ability/{ability}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IEnumerable<Pokemon>> GetPokemonByAbility(string ability)
    {
        try
        {
            var pokemons = _pokemonService.GetPokemonByAbility(ability);
            return Ok(pokemons);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Augmente le niveau d'un Pokémon
    /// </summary>
    [HttpPatch("{id:int}/level-up")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<Pokemon> LevelUpPokemon(int id)
    {
        try
        {
            _pokemonService.LevelUpPokemon(id);
            var updatedPokemon = _pokemonService.GetPokemonById(id);
            return Ok(updatedPokemon);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("introuvable"))
                return NotFound(new { message = ex.Message });
            else
                return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Change la capacité d'un Pokémon
    /// </summary>
    [HttpPatch("{id:int}/ability")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<Pokemon> ChangePokemonAbility(int id, [FromBody] ChangeAbilityRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _pokemonService.ChangePokemonAbility(id, request.NewAbility);
            var updatedPokemon = _pokemonService.GetPokemonById(id);
            return Ok(updatedPokemon);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }
}

// DTOs pour les requêtes
public class CreatePokemonRequest
{

    [Length(5,10)]
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public string Ability { get; set; } = string.Empty;
}

public class UpdatePokemonRequest
{
        [Length(5,10)]
    public string Name { get; set; } = string.Empty;
    public string Type { get; } = string.Empty;
    public int Level { get; set; } = 1;
    public string Ability { get; set; } = string.Empty;
}

public class ChangeAbilityRequest
{
    public string NewAbility { get; set; } = string.Empty;
}
