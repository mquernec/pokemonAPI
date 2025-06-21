using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

[Route("api/[controller]")]
[ApiController]
public class MonitoringController : ControllerBase
{
    /// <summary>
    /// Affiche les statistiques de performance en temps réel
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<object> GetStatistics()
    {
        try
        {
            // Déclencher l'affichage des statistiques dans la console
            RequestStatisticsMiddleware.DisplayCurrentStatistics();
            
            return Ok(new { 
                message = "Statistiques affichées dans la console du serveur",
                timestamp = DateTime.UtcNow,
                note = "Consultez la sortie standard pour voir les détails"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Test de performance - endpoint de test pour générer des statistiques
    /// </summary>
    [HttpGet("test/{delay:int?}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> PerformanceTest(int delay = 100)
    {
        try
        {
            // Simuler un délai pour tester les mesures de performance
            if (delay > 0 && delay <= 5000)
            {
                await Task.Delay(delay);
            }

            return Ok(new 
            { 
                message = $"Test de performance terminé avec un délai de {delay}ms",
                timestamp = DateTime.UtcNow,
                delay = delay
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Test d'erreur - endpoint de test pour générer des erreurs et tester la gestion
    /// </summary>
    [HttpGet("test-error/{type?}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult TestError(string type = "400")
    {
        return type.ToLower() switch
        {
            "400" or "bad" => BadRequest(new { message = "Erreur 400 - Bad Request générée pour test" }),
            "404" or "notfound" => NotFound(new { message = "Erreur 404 - Not Found générée pour test" }),
            "500" or "server" => StatusCode(500, new { message = "Erreur 500 - Server Error générée pour test" }),
            "exception" => throw new InvalidOperationException("Exception de test générée volontairement"),
            _ => BadRequest(new { message = "Type d'erreur non reconnu. Utilisez: 400, 404, 500, exception" })
        };
    }

    /// <summary>
    /// Informations sur le serveur et l'application
    /// </summary>
    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<object> GetServerInfo()
    {
        try
        {
            var info = new
            {
                Application = "Pokemon API",
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                MachineName = Environment.MachineName,
                ProcessorCount = Environment.ProcessorCount,
                WorkingSet = Environment.WorkingSet,
                UpTime = DateTime.UtcNow.Subtract(Process.GetCurrentProcess().StartTime.ToUniversalTime()),
                RequestTime = DateTime.UtcNow,
                TimeZone = TimeZoneInfo.Local.DisplayName
            };

            Console.WriteLine($"\n🖥️  ═══════════════ INFORMATIONS SERVEUR ═══════════════");
            Console.WriteLine($"📱 Application: {info.Application} v{info.Version}");
            Console.WriteLine($"🌍 Environnement: {info.Environment}");
            Console.WriteLine($"💻 Machine: {info.MachineName}");
            Console.WriteLine($"⚡ Processeurs: {info.ProcessorCount}");
            Console.WriteLine($"💾 Mémoire utilisée: {info.WorkingSet / 1024 / 1024} MB");
            Console.WriteLine($"⏰ Temps de fonctionnement: {info.UpTime}");
            Console.WriteLine($"🌐 Fuseau horaire: {info.TimeZone}");
            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

            return Ok(info);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Affiche les statistiques des ActionFilters
    /// </summary>
    [HttpGet("action-statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<object> GetActionStatistics()
    {
        try
        {
            // Déclencher l'affichage des statistiques des actions dans la console
            ActionStatisticsFilter.DisplayCurrentActionStatistics();
            
            // Récupérer les données pour l'API
            var statsData = ActionStatisticsFilter.GetStatisticsData();
            
            return Ok(new 
            { 
                message = "Statistiques des actions récupérées",
                timestamp = DateTime.UtcNow,
                consoleDisplay = "Statistiques détaillées affichées dans la console",
                data = statsData
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }
}
