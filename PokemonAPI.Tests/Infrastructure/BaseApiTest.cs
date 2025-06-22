using System.Net;
using System.Text;

namespace PokemonAPI.Tests.Infrastructure
{
    /// <summary>
    /// Classe de base pour les tests d'intégration d'API
    /// </summary>
    public abstract class BaseApiTest : IDisposable
    {
        protected readonly HttpClient _httpClient;
        protected readonly CustomWebApplicationFactory<TestProgram> _factory;

        protected BaseApiTest()
        {
            _factory = new CustomWebApplicationFactory<TestProgram>();
            _httpClient = _factory.CreateClient();
            
            // Configuration commune pour tous les tests
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        /// <summary>
        /// Crée le contenu JSON pour les requêtes POST/PUT
        /// </summary>
        protected StringContent CreateJsonContent(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        /// <summary>
        /// Désérialise la réponse HTTP en objet
        /// </summary>
        protected async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }

        /// <summary>
        /// Vérifie que la réponse est un succès et retourne le contenu désérialisé
        /// </summary>
        protected async Task<T> GetSuccessResponse<T>(HttpResponseMessage response)
        {
            response.Should().NotBeNull();
            response.IsSuccessStatusCode.Should().BeTrue($"Expected success status code but got {response.StatusCode}");
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            
            return JsonConvert.DeserializeObject<T>(content);
        }

        /// <summary>
        /// Vérifie que la réponse a le code d'erreur attendu
        /// </summary>
        protected void AssertErrorResponse(HttpResponseMessage response, HttpStatusCode expectedStatusCode)
        {
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(expectedStatusCode);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            _factory?.Dispose();
        }
    }
}
