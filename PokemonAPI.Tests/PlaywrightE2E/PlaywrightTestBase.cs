using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

namespace PokemonAPI.Tests.PlaywrightE2E
{
    /// <summary>
    /// Classe de base pour les tests Playwright E2E
    /// Fournit la configuration commune et les utilitaires pour les tests d'authentification
    /// </summary>
    [TestClass]
    public abstract class PlaywrightTestBase : PageTest
    {
        protected const string BaseUrl = "http://localhost:5183";
        protected const string ApiBaseUrl = "http://localhost:5183/api";
        
        /// <summary>
        /// Configuration globale pour tous les tests Playwright
        /// </summary>
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            // Configuration globale des navigateurs et options
        }

        /// <summary>
        /// Initialisation avant chaque test
        /// </summary>
        [TestInitialize]
        public async Task TestInitialize()
        {
            // Configuration spécifique du contexte de la page
            await Context.GrantPermissionsAsync(new[] { "clipboard-read", "clipboard-write" });
            
            // Configuration des timeouts pour les tests E2E
            Context.SetDefaultTimeout(30000); // 30 secondes
        }

        /// <summary>
        /// Utilitaire pour effectuer des appels API REST
        /// </summary>
        protected async Task<IAPIResponse> MakeApiRequestAsync(
            string method, 
            string endpoint, 
            object? data = null, 
            Dictionary<string, string>? headers = null)
        {
            var requestOptions = new APIRequestContextOptions
            {
                Method = method,
                BaseURL = ApiBaseUrl
            };

            if (data != null)
            {
                requestOptions.DataObject = data;
                headers ??= new Dictionary<string, string>();
                headers["Content-Type"] = "application/json";
            }

            if (headers != null)
            {
                requestOptions.Headers = headers;
            }

            return await Context.APIRequest.FetchAsync(endpoint, requestOptions);
        }

        /// <summary>
        /// Utilitaire pour l'authentification et récupération du token
        /// </summary>
        protected async Task<string> AuthenticateAndGetTokenAsync(string username, string password)
        {
            var loginData = new
            {
                username = username,
                password = password
            };

            var response = await MakeApiRequestAsync("POST", "/auth/login", loginData);
            
            response.Ok.Should().BeTrue($"Login should succeed, but got status {response.Status}");
            
            var responseBody = await response.JsonAsync();
            var token = responseBody?.GetProperty("token").GetString();
            
            token.Should().NotBeNullOrEmpty("Token should be returned from login");
            
            return token!;
        }

        /// <summary>
        /// Utilitaire pour créer des headers d'authentification
        /// </summary>
        protected Dictionary<string, string> CreateAuthHeaders(string token)
        {
            return new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {token}"
            };
        }

        /// <summary>
        /// Vérifie si l'API est accessible
        /// </summary>
        protected async Task<bool> IsApiHealthyAsync()
        {
            try
            {
                var response = await MakeApiRequestAsync("GET", "/health");
                return response.Ok;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Nettoyage après chaque test
        /// </summary>
        [TestCleanup]
        public async Task TestCleanup()
        {
            // Nettoyage des données de test si nécessaire
            await Task.CompletedTask;
        }
    }
}
