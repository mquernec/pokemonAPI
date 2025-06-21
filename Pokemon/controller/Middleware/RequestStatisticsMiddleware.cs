using System.Collections.Concurrent;
using System.Text.Json;

public class RequestStatisticsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestStatisticsMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, RequestStats> _statistics = new();
    private static readonly object _lockObject = new object();
    private static DateTime _lastStatsDisplay = DateTime.UtcNow;
    private static long _totalRequests = 0;

    public RequestStatisticsMiddleware(RequestDelegate next, ILogger<RequestStatisticsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var requestKey = $"{context.Request.Method} {context.Request.Path}";
        
        Interlocked.Increment(ref _totalRequests);

        try
        {
            await _next(context);
            stopwatch.Stop();

            // Mettre à jour les statistiques
            UpdateStatistics(requestKey, stopwatch.ElapsedMilliseconds, context.Response.StatusCode, true);
        }
        catch (Exception)
        {
            stopwatch.Stop();
            UpdateStatistics(requestKey, stopwatch.ElapsedMilliseconds, 500, false);
            throw;
        }

        // Afficher les statistiques toutes les 10 requêtes ou toutes les 30 secondes
        if (_totalRequests % 10 == 0 || DateTime.UtcNow.Subtract(_lastStatsDisplay).TotalSeconds > 30)
        {
            DisplayStatistics();
        }
    }

    private void UpdateStatistics(string requestKey, long elapsedMs, int statusCode, bool success)
    {
        _statistics.AddOrUpdate(requestKey, 
            new RequestStats(requestKey, elapsedMs, statusCode, success),
            (key, existing) => existing.Update(elapsedMs, statusCode, success));
    }

    private void DisplayStatistics()
    {
        lock (_lockObject)
        {
            if (DateTime.UtcNow.Subtract(_lastStatsDisplay).TotalSeconds < 5) return; // Éviter les affichages trop fréquents
            
            _lastStatsDisplay = DateTime.UtcNow;

            Console.WriteLine($"\n🏆 ═══════════════ STATISTIQUES EN TEMPS RÉEL ═══════════════");
            Console.WriteLine($"📊 Total des requêtes: {_totalRequests}");
            Console.WriteLine($"⏰ Mise à jour: {DateTime.Now:HH:mm:ss}");
            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            var sortedStats = _statistics.Values
                .OrderByDescending(s => s.Count)
                .Take(10);

            Console.WriteLine($"{"Endpoint",-35} {"Nb",-5} {"Moy",-6} {"Min",-6} {"Max",-6} {"Err",-5} {"Taux",-6}");
            Console.WriteLine(
                new string('─', 35) + " " +
                new string('─', 5) + " " +
                new string('─', 6) + " " +
                new string('─', 6) + " " +
                new string('─', 6) + " " +
                new string('─', 5) + " " +
                new string('─', 6)
            );

            foreach (var stat in sortedStats)
            {
                var endpoint = stat.Endpoint.Length > 32 ? stat.Endpoint.Substring(0, 29) + "..." : stat.Endpoint;
                var successRate = stat.Count > 0 ? (double)(stat.Count - stat.ErrorCount) / stat.Count * 100 : 0;
                
                Console.WriteLine($"{endpoint,-35} {stat.Count,-5} {stat.AverageTime,-6:F0}ms {stat.MinTime,-6}ms {stat.MaxTime,-6}ms {stat.ErrorCount,-5} {successRate,-6:F1}%");
            }

            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
        }
    }

    public static void DisplayCurrentStatistics()
    {
        lock (_lockObject)
        {
            Console.WriteLine($"\n📈 ═══════════════ STATISTIQUES DÉTAILLÉES ═══════════════");
            Console.WriteLine($"📊 Total des requêtes: {_totalRequests}");
            Console.WriteLine($"🎯 Endpoints uniques: {_statistics.Count}");
            Console.WriteLine($"⏰ Capture: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            foreach (var stat in _statistics.Values.OrderByDescending(s => s.Count))
            {
                Console.WriteLine($"\n🎯 {stat.Endpoint}");
                Console.WriteLine($"   📊 Requêtes: {stat.Count}");
                Console.WriteLine($"   ⏱️  Temps moyen: {stat.AverageTime:F2} ms");
                Console.WriteLine($"   ⚡ Temps min: {stat.MinTime} ms");
                Console.WriteLine($"   🐌 Temps max: {stat.MaxTime} ms");
                Console.WriteLine($"   ❌ Erreurs: {stat.ErrorCount}");
                Console.WriteLine($"   ✅ Taux de succès: {(stat.Count > 0 ? (double)(stat.Count - stat.ErrorCount) / stat.Count * 100 : 0):F1}%");
            }
            Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
        }
    }
}

public class RequestStats
{
    public string Endpoint { get; }
    public long Count { get; private set; }
    public long TotalTime { get; private set; }
    public long MinTime { get; private set; }
    public long MaxTime { get; private set; }
    public long ErrorCount { get; private set; }
    public double AverageTime => Count > 0 ? (double)TotalTime / Count : 0;

    public RequestStats(string endpoint, long elapsedMs, int statusCode, bool success)
    {
        Endpoint = endpoint;
        Count = 1;
        TotalTime = elapsedMs;
        MinTime = elapsedMs;
        MaxTime = elapsedMs;
        ErrorCount = success ? 0 : 1;
    }

    public RequestStats Update(long elapsedMs, int statusCode, bool success)
    {
        Count++;
        TotalTime += elapsedMs;
        MinTime = Math.Min(MinTime, elapsedMs);
        MaxTime = Math.Max(MaxTime, elapsedMs);
        if (!success) ErrorCount++;
        return this;
    }
}

public static class RequestStatisticsMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestStatistics(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestStatisticsMiddleware>();
    }
}
