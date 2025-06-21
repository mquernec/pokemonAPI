using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

public class ExecutionTimeActionFilter : ActionFilterAttribute
{
    private readonly ILogger<ExecutionTimeActionFilter> _logger;    private Stopwatch? _stopwatch;
    private DateTime _startTime;
    private string? _actionName;
    private string? _controllerName;

    public ExecutionTimeActionFilter(ILogger<ExecutionTimeActionFilter> logger)
    {
        _logger = logger;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        _stopwatch = Stopwatch.StartNew();
        _startTime = DateTime.UtcNow;
        _controllerName = context.Controller.GetType().Name.Replace("Controller", "");
        _actionName = context.ActionDescriptor.DisplayName ?? "Unknown";
        
        var httpContext = context.HttpContext;
        var method = httpContext.Request.Method;
        var path = httpContext.Request.Path;
        var queryString = httpContext.Request.QueryString.HasValue ? httpContext.Request.QueryString.Value : "";
        var userAgent = httpContext.Request.Headers.UserAgent.FirstOrDefault() ?? "Unknown";
        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        Console.WriteLine($"[{_startTime:yyyy-MM-dd HH:mm:ss.fff}] 🎯 ACTION DÉBUT - {_controllerName}.{context.ActionDescriptor.RouteValues["action"]}");
        Console.WriteLine($"  🌐 {method} {path}{queryString}");
        Console.WriteLine($"  📱 User-Agent: {userAgent}");
        Console.WriteLine($"  🔗 IP: {remoteIp}");
        
        // Log des paramètres d'action
        if (context.ActionArguments.Any())
        {
            Console.WriteLine($"  📋 Paramètres:");
            foreach (var param in context.ActionArguments)
            {
                var valueStr = param.Value?.ToString() ?? "null";
                if (valueStr.Length > 100) valueStr = valueStr.Substring(0, 97) + "...";
                Console.WriteLine($"     • {param.Key}: {valueStr}");
            }
        }

        base.OnActionExecuting(context);
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        _stopwatch?.Stop();
        var elapsedMs = _stopwatch?.ElapsedMilliseconds ?? 0;
        var httpContext = context.HttpContext;
        var method = httpContext.Request.Method;
        var path = httpContext.Request.Path;
        var statusCode = httpContext.Response.StatusCode;

        var (emoji, statusText) = GetStatusInfo(statusCode);
        var actionName = context.ActionDescriptor.RouteValues["action"] ?? "Unknown";

        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] {emoji} ACTION FIN - {_controllerName}.{actionName}");
        Console.WriteLine($"  ⏱️  Temps d'exécution: {elapsedMs} ms");
        Console.WriteLine($"  📊 Status: {statusCode} {statusText}");

        // Gestion des exceptions
        if (context.Exception != null)
        {
            Console.WriteLine($"  💥 Exception: {context.Exception.GetType().Name}");
            Console.WriteLine($"  📝 Message: {context.Exception.Message}");
            
            _logger.LogError(context.Exception, 
                "Action failed: {Controller}.{Action} after {ElapsedMs} ms", 
                _controllerName, actionName, elapsedMs);
        }
        else
        {
            // Alertes pour les actions lentes
            if (elapsedMs > 500) // Seuil plus bas pour les actions (500ms au lieu de 1000ms)
            {
                Console.WriteLine($"  ⚠️  ATTENTION: Action lente détectée! ({elapsedMs} ms)");
                _logger.LogWarning("Slow action detected: {Controller}.{Action} took {ElapsedMs} ms", 
                    _controllerName, actionName, elapsedMs);
            }

            if (statusCode >= 400)
            {
                Console.WriteLine($"  ❌ ERREUR: Code de statut {statusCode}");
                _logger.LogWarning("Error response: {Controller}.{Action} returned {StatusCode}", 
                    _controllerName, actionName, statusCode);
            }

            _logger.LogInformation("Action completed: {Controller}.{Action} {StatusCode} in {ElapsedMs} ms", 
                _controllerName, actionName, statusCode, elapsedMs);
        }

        Console.WriteLine($"  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        base.OnActionExecuted(context);
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

// Version sans injection de dépendance pour utilisation comme attribut
public class ExecutionTimeAttribute : ActionFilterAttribute
{    private Stopwatch? _stopwatch;
    private DateTime _startTime;
    private string? _controllerName;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        _stopwatch = Stopwatch.StartNew();
        _startTime = DateTime.UtcNow;
        _controllerName = context.Controller.GetType().Name.Replace("Controller", "");
        
        var httpContext = context.HttpContext;
        var method = httpContext.Request.Method;
        var path = httpContext.Request.Path;
        var actionName = context.ActionDescriptor.RouteValues["action"] ?? "Unknown";

        Console.WriteLine($"[{_startTime:yyyy-MM-dd HH:mm:ss.fff}] 🎯 [{_controllerName}] DÉBUT {actionName}");
        Console.WriteLine($"  🌐 {method} {path}");

        base.OnActionExecuting(context);
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        _stopwatch?.Stop();
        var elapsedMs = _stopwatch?.ElapsedMilliseconds ?? 0;
        var statusCode = context.HttpContext.Response.StatusCode;
        var actionName = context.ActionDescriptor.RouteValues["action"] ?? "Unknown";

        var (emoji, _) = GetStatusInfo(statusCode);

        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] {emoji} [{_controllerName}] FIN {actionName} - {elapsedMs}ms ({statusCode})");

        if (context.Exception != null)
        {
            Console.WriteLine($"  💥 Exception: {context.Exception.Message}");
        }
        else if (elapsedMs > 500)
        {
            Console.WriteLine($"  ⚠️  Action lente: {elapsedMs}ms");
        }

        base.OnActionExecuted(context);
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
