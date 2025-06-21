using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Toutes les actions nécessitent une authentification
[ServiceFilter(typeof(ExecutionTimeActionFilter))] // Trace toutes les actions de combat
public class BattleController : ControllerBase
{
    private readonly IBattleService _battleService;
    private readonly ITrainerService _trainerService;
    private readonly IPokemonService _pokemonService;

    public BattleController(IBattleService battleService, ITrainerService trainerService, IPokemonService pokemonService)
    {
        _battleService = battleService;
        _trainerService = trainerService;
        _pokemonService = pokemonService;
    }

    /// <summary>
    /// Récupère tous les combats
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IEnumerable<Battle>> GetAllBattles()
    {
        try
        {
            var battles = _battleService.GetAllBattles();
            return Ok(battles);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Récupère un combat par son ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<Battle> GetBattleById(int id)
    {
        try
        {
            var battle = _battleService.GetBattleById(id);
            if (battle == null)
            {
                return NotFound(new { message = $"Combat avec l'ID {id} introuvable." });
            }
            return Ok(battle);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Récupère les combats récents
    /// </summary>
    [HttpGet("recent")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IEnumerable<Battle>> GetRecentBattles([FromQuery] int days = 30)
    {
        try
        {
            var battles = _battleService.GetRecentBattles(days);
            return Ok(battles);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Crée un nouveau combat entre deux dresseurs
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<Battle> CreateBattle([FromBody] CreateBattleRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var battle = _battleService.CreateBattle(request.Trainer1Id, request.Trainer2Id, request.Location);
            return CreatedAtAction(nameof(GetBattleById), new { id = battle.Id }, battle);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }    /// <summary>
    /// Démarre un combat
    /// </summary>
    [HttpPatch("{id:int}/start")]
    [MonitorPerformance(ActionName = "CriticalBattleStart")] // Action critique à surveiller
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<Battle> StartBattle(int id)
    {
        try
        {
            _battleService.StartBattle(id);
            var battle = _battleService.GetBattleById(id);
            return Ok(new { message = "Combat démarré avec succès", battle });
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
    /// Ajoute un round à un combat
    /// </summary>
    [HttpPost("{id:int}/rounds")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<Battle> AddRound(int id, [FromBody] AddRoundRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _battleService.AddRound(id, request.Pokemon1Id, request.Pokemon2Id, request.WinnerPokemonId, request.Description);
            var battle = _battleService.GetBattleById(id);
            return Ok(new { message = "Round ajouté avec succès", battle });
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
    /// Définit le gagnant d'un combat
    /// </summary>
    [HttpPatch("{id:int}/winner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<Battle> SetBattleWinner(int id, [FromBody] SetWinnerRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _battleService.SetBattleWinner(id, request.WinnerId);
            var battle = _battleService.GetBattleById(id);
            return Ok(new { message = $"Combat terminé - Gagnant: {battle.WinnerName}", battle });
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
    /// Définit un combat comme match nul
    /// </summary>
    [HttpPatch("{id:int}/draw")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<Battle> SetBattleDraw(int id)
    {
        try
        {
            _battleService.SetBattleDraw(id);
            var battle = _battleService.GetBattleById(id);
            return Ok(new { message = "Combat terminé en match nul", battle });
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
    /// Annule un combat
    /// </summary>
    [HttpPatch("{id:int}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<Battle> CancelBattle(int id)
    {
        try
        {
            _battleService.CancelBattle(id);
            var battle = _battleService.GetBattleById(id);
            return Ok(new { message = "Combat annulé", battle });
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
    /// Ajoute des notes à un combat
    /// </summary>
    [HttpPatch("{id:int}/notes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<Battle> AddBattleNotes(int id, [FromBody] AddNotesRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _battleService.AddBattleNotes(id, request.Notes);
            var battle = _battleService.GetBattleById(id);
            return Ok(new { message = "Notes ajoutées au combat", battle });
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
    /// Supprime un combat
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult DeleteBattle(int id)
    {
        try
        {
            _battleService.DeleteBattle(id);
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
    /// Récupère les statistiques générales des combats
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<object> GetBattleStatistics()
    {
        try
        {
            var allBattles = _battleService.GetAllBattles();
            var totalBattles = allBattles.Count;
            var completedBattles = allBattles.Count(b => b.Result == BattleResult.Completed);
            var drawBattles = allBattles.Count(b => b.Result == BattleResult.Draw);
            var cancelledBattles = allBattles.Count(b => b.Result == BattleResult.Cancelled);
            var inProgressBattles = allBattles.Count(b => b.Result == BattleResult.InProgress);

            var statistics = new
            {
                TotalBattles = totalBattles,
                CompletedBattles = completedBattles,
                DrawBattles = drawBattles,
                CancelledBattles = cancelledBattles,
                InProgressBattles = inProgressBattles,
                CompletionRate = totalBattles > 0 ? (double)completedBattles / totalBattles * 100 : 0
            };

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }
}

// DTOs pour les requêtes de combat
public class CreateBattleRequest
{
    public int Trainer1Id { get; set; }
    public int Trainer2Id { get; set; }
    public string Location { get; set; } = string.Empty;
}

public class AddRoundRequest
{
    public int Pokemon1Id { get; set; }
    public int Pokemon2Id { get; set; }
    public int? WinnerPokemonId { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class SetWinnerRequest
{
    public int WinnerId { get; set; }
}

public class AddNotesRequest
{
    public string Notes { get; set; } = string.Empty;
}
