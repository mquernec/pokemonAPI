using System.Diagnostics;

public class ExecutionTimeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExecutionTimeMiddleware> _logger;

    public ExecutionTimeMiddleware(RequestDelegate next, ILogger<ExecutionTimeMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestTime = DateTime.UtcNow;
        
        // Informations sur la requête
        var method = context.Request.Method;
        var path = context.Request.Path;
        var queryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : "";
        var userAgent = context.Request.Headers.UserAgent.FirstOrDefault() ?? "Unknown";
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        try
        {
            // Log du début de la requête
            Console.WriteLine($"[{requestTime:yyyy-MM-dd HH:mm:ss.fff}] 🚀 DÉBUT - {method} {path}{queryString}");
            Console.WriteLine($"  📱 User-Agent: {userAgent}");
            Console.WriteLine($"  🌐 IP: {remoteIp}");

            // Exécuter la requête suivante dans le pipeline
            await _next(context);

            stopwatch.Stop();

            // Informations sur la réponse
            var statusCode = context.Response.StatusCode;
            var responseTime = stopwatch.ElapsedMilliseconds;

            // Déterminer l'emoji et la couleur selon le code de statut
            var (emoji, statusText) = GetStatusInfo(statusCode);

            // Log de fin avec temps d'exécution
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] {emoji} FIN - {method} {path}{queryString}");
            Console.WriteLine($"  ⏱️  Temps d'exécution: {responseTime} ms");
            Console.WriteLine($"  📊 Status: {statusCode} {statusText}");
            
            // Alertes pour les requêtes lentes ou les erreurs
            if (responseTime > 1000)
            {
                Console.WriteLine($"  ⚠️  ATTENTION: Requête lente détectée! ({responseTime} ms)");
                _logger.LogWarning("Slow request detected: {Method} {Path} took {ElapsedMs} ms", 
                    method, path, responseTime);
            }

            if (statusCode >= 400)
            {
                Console.WriteLine($"  ❌ ERREUR: Code de statut {statusCode}");
                _logger.LogWarning("Error response: {Method} {Path} returned {StatusCode}", 
                    method, path, statusCode);
            }

            // Log dans les logs structurés
            _logger.LogInformation("Request completed: {Method} {Path} {StatusCode} in {ElapsedMs} ms", 
                method, path, statusCode, responseTime);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var responseTime = stopwatch.ElapsedMilliseconds;

            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] 💥 EXCEPTION - {method} {path}{queryString}");
            Console.WriteLine($"  ⏱️  Temps avant exception: {responseTime} ms");
            Console.WriteLine($"  ❌ Exception: {ex.GetType().Name}");
            Console.WriteLine($"  📝 Message: {ex.Message}");

            _logger.LogError(ex, "Request failed: {Method} {Path} after {ElapsedMs} ms", 
                method, path, responseTime);

            throw; // Re-lancer l'exception
        }
        finally
        {
            Console.WriteLine($"  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"); // Séparateur
        }
    }

    private static (string emoji, string statusText) GetStatusInfo(int statusCode)
    {
        return statusCode switch
        {
            >= 200 and < 300 => ("✅", "Success"),
            >= 300 and < 400 => ("🔄", "Redirection"),
            >= 400 and < 500 => ("❌", "Client Error"),
            >= 500 => ("💥", "Server Error"),
            _ => ("❓", "Unknown")
        };
    }
}

// Extension method pour faciliter l'enregistrement du middleware
public static class ExecutionTimeMiddlewareExtensions
{
    public static IApplicationBuilder UseExecutionTimeTracing(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExecutionTimeMiddleware>();
    }
}
