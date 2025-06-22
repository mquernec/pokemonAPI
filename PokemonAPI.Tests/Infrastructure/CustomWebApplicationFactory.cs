using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace PokemonAPI.Tests.Infrastructure
{
    /// <summary>
    /// Factory personnalisée pour créer une instance de test de l'application web
    /// Compatible avec .NET 9 et ses nouvelles fonctionnalités
    /// </summary>
    /// <typeparam name="TProgram">Type du programme à tester</typeparam>
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Configuration de test - remplacez les services par des mocks si nécessaire
                // Par exemple, remplacer la base de données par une base en mémoire
                  // Utiliser FakeTimeProvider pour les tests temporels (.NET 9)
                var fakeTime = new DateTimeOffset(2025, 6, 22, 12, 0, 0, TimeSpan.Zero);
                services.AddSingleton<TimeProvider>(new FakeTimeProvider(fakeTime));
                
                // Configuration des services avec clés (.NET 9)
                services.AddKeyedSingleton<ITestService, MockTestService>("test");
                
                // Configurer le logging pour les tests
                services.AddLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Warning);
                    // Nouveau logging structuré en .NET 9
                    logging.AddJsonConsole(options =>
                    {
                        options.UseUtcTimestamp = true;
                        options.JsonWriterOptions = new System.Text.Json.JsonWriterOptions
                        {
                            Indented = false
                        };
                    });
                });

                // Configuration pour les métriques (.NET 9)
                services.AddMetrics();
                  // Configuration pour HTTP/3 en test
                services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
                {
                    // Configuration HTTP/3 pour .NET 9 - plus d'EnableAltSvc obsolète
                    options.ConfigureEndpointDefaults(listenOptions =>
                    {
                        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2AndHttp3;
                    });
                });
            });
            
            builder.UseEnvironment("Testing");
            
            // Configuration des nouvelles fonctionnalités .NET 9
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.test.json", optional: true);
            });
        }
    }

    // Interface et mock pour les services avec clés
    public interface ITestService
    {
        string GetTestData();
    }    public class MockTestService : ITestService
    {
        public string GetTestData() => "Mock data for testing";
    }

    /// <summary>
    /// Classe Program pour les tests - Compatible .NET 9
    /// </summary>
    public class TestProgram
    {
        // Cette classe sert de point d'entrée pour les tests
        // Elle remplace la référence directe au Program des projets testés
    }
}
