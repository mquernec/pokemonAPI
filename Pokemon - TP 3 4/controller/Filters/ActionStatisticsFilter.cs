using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Concurrent;
using System.Text.Json;

public class ActionStatisticsFilter : ActionFilterAttribute
{
    private static readonly ConcurrentDictionary<string, ActionStats> _actionStatistics = new();
    private static readonly object _lockObject = new object();
    private static DateTime _lastStatsDisplay = DateTime.UtcNow;
    private static long _totalActions = 0;
      private System.Diagnostics.Stopwatch? _stopwatch;
    private string? _actionKey;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        _stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var controllerName = context.Controller.GetType().Name.Replace("Controller", "");
        var actionName = context.ActionDescriptor.RouteValues["action"] ?? "Unknown";
        _actionKey = $"{controllerName}.{actionName}";
        
        Interlocked.Increment(ref _totalActions);
        
        base.OnActionExecuting(context);
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        _stopwatch?.Stop();
        var elapsedMs = _stopwatch?.ElapsedMilliseconds ?? 0;
        var statusCode = context.HttpContext.Response.StatusCode;
        var success = context.Exception == null && statusCode < 400;        // Mettre à jour les statistiques
        if (!string.IsNullOrEmpty(_actionKey))
        {
            UpdateActionStatistics(_actionKey, elapsedMs, statusCode, success);
        }

        // Afficher les statistiques toutes les 5 actions ou toutes les 30 secondes
        if (_totalActions % 5 == 0 || DateTime.UtcNow.Subtract(_lastStatsDisplay).TotalSeconds > 30)
        {
            DisplayActionStatistics();
        }

        base.OnActionExecuted(context);
    }

    private void UpdateActionStatistics(string actionKey, long elapsedMs, int statusCode, bool success)
    {
        _actionStatistics.AddOrUpdate(actionKey,
            new ActionStats(actionKey, elapsedMs, statusCode, success),
            (key, existing) => existing.Update(elapsedMs, statusCode, success));
    }

    private void DisplayActionStatistics()
    {
        lock (_lockObject)
        {
            if (DateTime.UtcNow.Subtract(_lastStatsDisplay).TotalSeconds < 3) return;
            
            _lastStatsDisplay = DateTime.UtcNow;

            Console.WriteLine($"\n🎯 ═══════════════ STATISTIQUES ACTIONS ═══════════════");
            Console.WriteLine($"📊 Total des actions: {_totalActions}");
            Console.WriteLine($"⏰ Mise à jour: {DateTime.Now:HH:mm:ss}");
            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            var sortedStats = _actionStatistics.Values
                .OrderByDescending(s => s.Count)
                .Take(8);            Console.WriteLine($"{"Action",-30} {"Nb",-4} {"Moy",-6} {"Min",-5} {"Max",-5} {"Err",-4} {"Taux",-6}");
            Console.WriteLine($"{new string('─', 30)} {new string('─', 4)} {new string('─', 6)} {new string('─', 5)} {new string('─', 5)} {new string('─', 4)} {new string('─', 6)}");

            foreach (var stat in sortedStats)
            {
                var actionName = stat.ActionName.Length > 27 ? stat.ActionName.Substring(0, 24) + "..." : stat.ActionName;
                var successRate = stat.Count > 0 ? (double)(stat.Count - stat.ErrorCount) / stat.Count * 100 : 0;
                
                Console.WriteLine($"{actionName,-30} {stat.Count,-4} {stat.AverageTime,-6:F0}ms {stat.MinTime,-5}ms {stat.MaxTime,-5}ms {stat.ErrorCount,-4} {successRate,-6:F1}%");
            }

            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
        }
    }

    public static void DisplayCurrentActionStatistics()
    {
        lock (_lockObject)
        {
            Console.WriteLine($"\n🎯 ═══════════════ STATISTIQUES DÉTAILLÉES ACTIONS ═══════════════");
            Console.WriteLine($"📊 Total des actions exécutées: {_totalActions}");
            Console.WriteLine($"🎭 Actions uniques: {_actionStatistics.Count}");
            Console.WriteLine($"⏰ Capture: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            foreach (var stat in _actionStatistics.Values.OrderByDescending(s => s.Count))
            {
                Console.WriteLine($"\n🎯 {stat.ActionName}");
                Console.WriteLine($"   📊 Exécutions: {stat.Count}");
                Console.WriteLine($"   ⏱️  Temps moyen: {stat.AverageTime:F2} ms");
                Console.WriteLine($"   ⚡ Temps min: {stat.MinTime} ms");
                Console.WriteLine($"   🐌 Temps max: {stat.MaxTime} ms");
                Console.WriteLine($"   ❌ Erreurs: {stat.ErrorCount}");
                Console.WriteLine($"   ✅ Taux de succès: {(stat.Count > 0 ? (double)(stat.Count - stat.ErrorCount) / stat.Count * 100 : 0):F1}%");
                
                // Affichage des codes de statut les plus fréquents
                var topStatusCodes = stat.StatusCodes
                    .GroupBy(sc => sc)
                    .OrderByDescending(g => g.Count())
                    .Take(3);
                    
                Console.Write($"   📋 Status codes: ");
                foreach (var statusGroup in topStatusCodes)
                {
                    Console.Write($"{statusGroup.Key}({statusGroup.Count()}) ");
                }
                Console.WriteLine();
            }
            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
        }
    }

    public static Dictionary<string, object> GetStatisticsData()
    {
        return new Dictionary<string, object>
        {
            ["totalActions"] = _totalActions,
            ["uniqueActions"] = _actionStatistics.Count,
            ["actions"] = _actionStatistics.Values.Select(s => new
            {
                name = s.ActionName,
                count = s.Count,
                averageTime = s.AverageTime,
                minTime = s.MinTime,
                maxTime = s.MaxTime,
                errorCount = s.ErrorCount,
                successRate = s.Count > 0 ? (double)(s.Count - s.ErrorCount) / s.Count * 100 : 0
            }).OrderByDescending(a => a.count).ToList()
        };
    }
}

public class ActionStats
{
    public string ActionName { get; } = string.Empty;
    public long Count { get; private set; }
    public long TotalTime { get; private set; }
    public long MinTime { get; private set; }
    public long MaxTime { get; private set; }
    public long ErrorCount { get; private set; }
    public List<int> StatusCodes { get; private set; } = new List<int>();
    public double AverageTime => Count > 0 ? (double)TotalTime / Count : 0;

    public ActionStats(string actionName, long elapsedMs, int statusCode, bool success)
    {
        ActionName = actionName;
        Count = 1;
        TotalTime = elapsedMs;
        MinTime = elapsedMs;
        MaxTime = elapsedMs;
        ErrorCount = success ? 0 : 1;
        StatusCodes = new List<int> { statusCode };
    }

    public ActionStats Update(long elapsedMs, int statusCode, bool success)
    {
        Count++;
        TotalTime += elapsedMs;
        MinTime = Math.Min(MinTime, elapsedMs);
        MaxTime = Math.Max(MaxTime, elapsedMs);
        if (!success) ErrorCount++;
        StatusCodes.Add(statusCode);
        
        // Garder seulement les 100 derniers codes de statut pour éviter une croissance excessive
        if (StatusCodes.Count > 100)
        {
            StatusCodes.RemoveRange(0, StatusCodes.Count - 100);
        }
        
        return this;
    }
}

// Attribut simple pour marquer les actions à surveiller
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class MonitorPerformanceAttribute : ActionFilterAttribute
{
    public string ActionName { get; set; }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var controllerName = context.Controller.GetType().Name.Replace("Controller", "");
        var actionName = ActionName ?? context.ActionDescriptor.RouteValues["action"] ?? "Unknown";
        
        Console.WriteLine($"🔍 [MONITOR] Surveillance activée pour {controllerName}.{actionName}");
        base.OnActionExecuting(context);
    }
}
