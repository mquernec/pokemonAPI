using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

namespace PokemonAPI.Tests.PlaywrightE2E;

/// <summary>
/// Classe de base pour les tests Playwright E2E
/// </summary>
[TestClass]
public abstract class PlaywrightTestBase : PageTest
{
    protected const string BaseUrl = "http://localhost:5183";
    
    /// <summary>
    /// Initialise le contexte de page pour chaque test
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        // Configuration par défaut pour tous les tests
        Page.SetDefaultTimeout(30000); // 30 secondes
    }

    /// <summary>
    /// Helper pour effectuer des requêtes HTTP avec Playwright
    /// </summary>
    /// <param name="method">Méthode HTTP</param>
    /// <param name="endpoint">Endpoint de l'API</param>
    /// <param name="data">Données à envoyer (pour POST/PUT)</param>
    /// <param name="headers">Headers HTTP supplémentaires</param>
    /// <returns>Réponse de l'API</returns>
    protected async Task<IAPIResponse> ApiRequestAsync(
        string method, 
        string endpoint, 
        object? data = null, 
        Dictionary<string, string>? headers = null)
    {
        var extraHeaders = headers ?? new Dictionary<string, string>();
        
        if (!extraHeaders.ContainsKey("Content-Type"))
        {
            extraHeaders["Content-Type"] = "application/json";
        }

        var options = new APIRequestNewContextOptions
        {
            BaseURL = BaseUrl,
            ExtraHTTPHeaders = extraHeaders
        };

        var request = await Playwright.APIRequest.NewContextAsync(options);

        return method.ToUpper() switch
        {
            "GET" => await request.GetAsync(endpoint),
            "POST" => await request.PostAsync(endpoint, new APIRequestContextOptions { DataObject = data }),
            "PUT" => await request.PutAsync(endpoint, new APIRequestContextOptions { DataObject = data }),
            "DELETE" => await request.DeleteAsync(endpoint),
            _ => throw new NotSupportedException($"Méthode HTTP non supportée: {method}")
        };
    }

    /// <summary>
    /// Helper pour extraire et parser le JSON d'une réponse
    /// </summary>
    /// <typeparam name="T">Type attendu pour la désérialisation</typeparam>
    /// <param name="response">Réponse de l'API</param>
    /// <returns>Objet désérialisé</returns>
    protected async Task<T?> ParseJsonResponseAsync<T>(IAPIResponse response)
    {
        var jsonText = await response.TextAsync();
        return System.Text.Json.JsonSerializer.Deserialize<T>(jsonText, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    /// <summary>
    /// Helper pour valider qu'une réponse est un succès HTTP
    /// </summary>
    /// <param name="response">Réponse à valider</param>
    /// <param name="expectedStatus">Code de statut attendu (200 par défaut)</param>
    protected static void AssertSuccessResponse(IAPIResponse response, int expectedStatus = 200)
    {
        Assert.IsTrue(response.Ok, $"Réponse HTTP non réussie. Status: {response.Status}");
        Assert.AreEqual(expectedStatus, response.Status, $"Code de statut inattendu. Attendu: {expectedStatus}, Reçu: {response.Status}");
    }

    /// <summary>
    /// Helper pour valider qu'une réponse a un code d'erreur spécifique
    /// </summary>
    /// <param name="response">Réponse à valider</param>
    /// <param name="expectedStatus">Code d'erreur attendu</param>
    protected static void AssertErrorResponse(IAPIResponse response, int expectedStatus)
    {
        Assert.IsFalse(response.Ok, "La réponse aurait dû être une erreur");
        Assert.AreEqual(expectedStatus, response.Status, $"Code d'erreur inattendu. Attendu: {expectedStatus}, Reçu: {response.Status}");
    }
}
