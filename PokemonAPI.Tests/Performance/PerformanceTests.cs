using System.Net;

namespace PokemonAPI.Tests.Performance
{
    /// <summary>
    /// Tests de performance pour l'API Pokemon
    /// </summary>
    [TestClass]    public class PerformanceTests : IDisposable
    {
        private HttpClient _httpClient;
        private CustomWebApplicationFactory<TestProgram> _factory;

        [TestInitialize]
        public void Setup()
        {
            _factory = new CustomWebApplicationFactory<TestProgram>();
            _httpClient = _factory.CreateClient();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        #region Tests de temps de réponse

        [TestMethod]
        public async Task GetAllPokemons_ShouldRespondWithinAcceptableTime()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            const int maxResponseTimeMs = 2000; // 2 secondes max

            // Act
            var response = await _httpClient.GetAsync("/api/pokemon");

            // Assert
            stopwatch.Stop();
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(maxResponseTimeMs, 
                $"API should respond within {maxResponseTimeMs}ms but took {stopwatch.ElapsedMilliseconds}ms");
        }

        [TestMethod]
        public async Task GetPokemonById_ShouldRespondQuickly()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            const int maxResponseTimeMs = 1000; // 1 seconde max

            // Act
            var response = await _httpClient.GetAsync("/api/pokemon/1");

            // Assert
            stopwatch.Stop();
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(maxResponseTimeMs);
        }

        [TestMethod]
        public async Task SearchOperations_ShouldRespondWithinAcceptableTime()
        {
            // Test des opérations de recherche qui peuvent être plus coûteuses
            var searchOperations = new[]
            {
                "/api/pokemon/type/Electric",
                "/api/pokemon/level?minLevel=1&maxLevel=50",
                "/api/pokemon/ability/Static",
                "/api/trainer/region/Kanto"
            };

            const int maxResponseTimeMs = 3000; // 3 secondes max pour les recherches

            foreach (var operation in searchOperations)
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                var response = await _httpClient.GetAsync(operation);
                
                stopwatch.Stop();
                response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
                stopwatch.ElapsedMilliseconds.Should().BeLessThan(maxResponseTimeMs,
                    $"Search operation {operation} should respond within {maxResponseTimeMs}ms but took {stopwatch.ElapsedMilliseconds}ms");
            }
        }

        #endregion

        #region Tests de charge

        [TestMethod]
        public async Task ConcurrentGetRequests_ShouldHandleLoad()
        {
            // Arrange
            const int numberOfConcurrentRequests = 50;
            const int maxResponseTimeMs = 5000; // 5 secondes max sous charge

            var tasks = new List<Task<(HttpResponseMessage Response, long ElapsedMs)>>();

            // Act
            for (int i = 0; i < numberOfConcurrentRequests; i++)
            {
                tasks.Add(MeasuredRequest("/api/pokemon"));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            results.Should().HaveCount(numberOfConcurrentRequests);
            
            // Toutes les requêtes devraient réussir ou être non autorisées
            results.Should().AllSatisfy(result => 
                result.Response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized));

            // 95% des requêtes devraient répondre dans le temps acceptable
            var successfulRequests = results.Where(r => r.Response.IsSuccessStatusCode).ToList();
            if (successfulRequests.Any())
            {
                var percentile95 = successfulRequests.OrderBy(r => r.ElapsedMs)
                    .Skip((int)(successfulRequests.Count * 0.95)).FirstOrDefault();
                
                if (percentile95.ElapsedMs > 0)
                {
                    percentile95.ElapsedMs.Should().BeLessThan(maxResponseTimeMs,
                        "95% of requests should complete within acceptable time under load");
                }
            }
        }

        [TestMethod]
        public async Task MixedConcurrentRequests_ShouldHandleLoad()
        {
            // Tester un mélange de différents types de requêtes
            const int numberOfEachType = 10;
            var tasks = new List<Task<(HttpResponseMessage Response, long ElapsedMs)>>();

            // Requêtes GET simples
            for (int i = 0; i < numberOfEachType; i++)
            {
                tasks.Add(MeasuredRequest("/api/pokemon"));
                tasks.Add(MeasuredRequest("/api/trainer"));
                tasks.Add(MeasuredRequest($"/api/pokemon/{(i % 10) + 1}"));
            }

            // Requêtes de recherche
            for (int i = 0; i < numberOfEachType; i++)
            {
                tasks.Add(MeasuredRequest("/api/pokemon/type/Electric"));
                tasks.Add(MeasuredRequest("/api/pokemon/level?minLevel=1&maxLevel=50"));
            }

            var results = await Task.WhenAll(tasks);

            // Toutes les requêtes devraient être traitées
            results.Should().HaveCount(numberOfEachType * 5);
            
            // Calculer les statistiques de performance
            var successfulRequests = results.Where(r => r.Response.IsSuccessStatusCode).ToList();
            if (successfulRequests.Any())
            {
                var averageResponseTime = successfulRequests.Average(r => r.ElapsedMs);
                var maxResponseTime = successfulRequests.Max(r => r.ElapsedMs);

                Console.WriteLine($"Average response time: {averageResponseTime}ms");
                Console.WriteLine($"Max response time: {maxResponseTime}ms");
                
                // Temps de réponse moyen ne devrait pas dépasser 3 secondes
                averageResponseTime.Should().BeLessThan(3000);
            }
        }

        #endregion

        #region Tests de stress

        [TestMethod]
        public async Task HighVolumeRequests_ShouldNotCauseErrors()
        {
            // Test avec un volume élevé de requêtes pour détecter les fuites mémoire ou autres problèmes
            const int numberOfRequests = 100;
            var successCount = 0;
            var errorCount = 0;

            for (int i = 0; i < numberOfRequests; i++)
            {
                try
                {
                    var response = await _httpClient.GetAsync("/api/pokemon");
                    if (response.IsSuccessStatusCode)
                        successCount++;
                    else if (response.StatusCode == HttpStatusCode.Unauthorized)
                        successCount++; // Compter comme succès si l'auth est requise
                    else
                        errorCount++;
                }
                catch
                {
                    errorCount++;
                }

                // Petite pause pour éviter de surcharger complètement
                if (i % 10 == 0)
                    await Task.Delay(10);
            }

            // Au moins 95% des requêtes devraient réussir
            var successRate = (double)successCount / numberOfRequests;
            successRate.Should().BeGreaterThan(0.95, 
                $"Success rate should be > 95% but was {successRate:P2} ({successCount}/{numberOfRequests})");
        }

        #endregion

        #region Tests de taille de réponse

        [TestMethod]
        public async Task ResponseSizes_ShouldBeReasonable()
        {
            var endpoints = new[]
            {
                "/api/pokemon",
                "/api/trainer",
                "/api/battle"
            };

            const int maxResponseSizeBytes = 10 * 1024 * 1024; // 10 MB max

            foreach (var endpoint in endpoints)
            {
                var response = await _httpClient.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsByteArrayAsync();
                    content.Length.Should().BeLessThan(maxResponseSizeBytes,
                        $"Response from {endpoint} should not exceed {maxResponseSizeBytes} bytes");
                }
            }
        }

        #endregion

        #region Méthodes utilitaires

        private async Task<(HttpResponseMessage Response, long ElapsedMs)> MeasuredRequest(string endpoint)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _httpClient.GetAsync(endpoint);
            stopwatch.Stop();
            
            return (response, stopwatch.ElapsedMilliseconds);
        }

        #endregion

        [TestCleanup]
        public void Cleanup()
        {
            _httpClient?.Dispose();
            _factory?.Dispose();
        }

        public void Dispose()
        {
            Cleanup();
        }
    }
}
