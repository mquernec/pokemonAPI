using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Toutes les actions nécessitent une authentification
[ExecutionTime] // Trace simple pour toutes les actions du contrôleur
public class TrainerController : ControllerBase
{
    private readonly ITrainerService _trainerService;
    private readonly IBattleService _battleService;

    public TrainerController(ITrainerService trainerService, IBattleService battleService)
    {
        _trainerService = trainerService;
        _battleService = battleService;
    }

    /// <summary>
    /// Récupère tous les dresseurs (accès restreint aux Trainers et Admins)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "TrainerOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IEnumerable<Trainer>> GetAllTrainers()
    {
        try
        {
            var trainers = _trainerService.GetAllTrainers();
            return Ok(trainers);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Récupère un dresseur par son ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<Trainer> GetTrainerById(int id)
    {
        try
        {
            var trainer = _trainerService.GetTrainerById(id);
            if (trainer == null)
            {
                return NotFound(new { message = $"Dresseur avec l'ID {id} introuvable." });
            }
            return Ok(trainer);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Récupère un dresseur par son nom
    /// </summary>
    [HttpGet("name/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<Trainer> GetTrainerByName(string name)
    {
        try
        {
            var trainer = _trainerService.GetTrainerByName(name);
            if (trainer == null)
            {
                return NotFound(new { message = $"Dresseur '{name}' introuvable." });
            }
            return Ok(trainer);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Récupère les dresseurs par région
    /// </summary>
    [HttpGet("region/{region}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IEnumerable<Trainer>> GetTrainersByRegion(string region)
    {
        try
        {
            var trainers = _trainerService.GetTrainersByRegion(region);
            return Ok(trainers);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Crée un nouveau dresseur
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<Trainer> CreateTrainer([FromBody] CreateTrainerRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var trainer = new Trainer(0, request.Name, request.Age, request.Region, request.BadgeCount);
            _trainerService.CreateTrainer(trainer);

            return CreatedAtAction(nameof(GetTrainerById), new { id = trainer.Id }, trainer);
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
    /// Met à jour un dresseur existant
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<Trainer> UpdateTrainer(int id, [FromBody] UpdateTrainerRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var trainer = new Trainer(id, request.Name, request.Age, request.Region, request.BadgeCount);
            _trainerService.UpdateTrainer(trainer);

            return Ok(trainer);
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
    /// Supprime un dresseur
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult DeleteTrainer(int id)
    {
        try
        {
            _trainerService.DeleteTrainer(id);
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
    /// Assigne un Pokémon à un dresseur
    /// </summary>
    [HttpPost("{id:int}/pokemon")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult AssignPokemonToTrainer(int id, [FromBody] AssignPokemonRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pokemon = new Pokemon(request.PokemonId, request.Name, request.Type, request.Level, request.Ability);
            _trainerService.AssignPokemonToTrainer(id, pokemon);

            var updatedTrainer = _trainerService.GetTrainerById(id);
            return Ok(new { message = $"Pokémon {pokemon.Name} assigné au dresseur {updatedTrainer.Name}", trainer = updatedTrainer });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
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
    /// Retire un Pokémon d'un dresseur
    /// </summary>
    [HttpDelete("{id:int}/pokemon/{pokemonId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult RemovePokemonFromTrainer(int id, int pokemonId)
    {
        try
        {
            var trainer = _trainerService.GetTrainerById(id);
            if (trainer == null)
            {
                return NotFound(new { message = $"Dresseur avec l'ID {id} introuvable." });
            }

            var pokemon = trainer.PokemonTeam.FirstOrDefault(p => p.Id == pokemonId);
            if (pokemon == null)
            {
                return NotFound(new { message = $"Pokémon avec l'ID {pokemonId} introuvable dans l'équipe du dresseur." });
            }

            _trainerService.RemovePokemonFromTrainer(id, pokemon);

            var updatedTrainer = _trainerService.GetTrainerById(id);
            return Ok(new { message = $"Pokémon {pokemon.Name} retiré de l'équipe du dresseur {updatedTrainer.Name}", trainer = updatedTrainer });
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
    /// Récupère les statistiques de combat d'un dresseur
    /// </summary>
    [HttpGet("{id:int}/statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<BattleStatistics> GetTrainerStatistics(int id)
    {
        try
        {
            var statistics = _battleService.GetTrainerStatistics(id);
            return Ok(statistics);
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
    /// Récupère l'historique des combats d'un dresseur
    /// </summary>
    [HttpGet("{id:int}/battles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IEnumerable<Battle>> GetTrainerBattles(int id)
    {
        try
        {
            var battles = _battleService.GetBattlesByTrainer(id);
            return Ok(battles);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Récupère l'historique des combats entre deux dresseurs
    /// </summary>
    [HttpGet("{id1:int}/battles/{id2:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IEnumerable<Battle>> GetBattleHistoryBetweenTrainers(int id1, int id2)
    {
        try
        {
            var battles = _battleService.GetBattleHistory(id1, id2);
            return Ok(battles);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }
}

// DTOs pour les requêtes de dresseurs
public class CreateTrainerRequest
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Region { get; set; } = string.Empty;
    public int BadgeCount { get; set; } = 0;
}

public class UpdateTrainerRequest
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Region { get; set; } = string.Empty;
    public int BadgeCount { get; set; } = 0;
}

public class AssignPokemonRequest
{
    public int PokemonId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public string Ability { get; set; } = string.Empty;
}
