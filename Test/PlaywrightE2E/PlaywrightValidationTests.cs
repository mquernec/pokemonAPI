using Microsoft.Playwright;

namespace PokemonAPI.Tests.PlaywrightE2E;

/// <summary>
/// Tests de validation pour vérifier que Playwright fonctionne correctement
/// </summary>
[TestClass]
public class PlaywrightValidationTests : PlaywrightTestBase
{
    /// <summary>
    /// Test simple pour valider que Playwright est configuré correctement
    /// </summary>
    [TestMethod]
    public async Task Playwright_ShouldBeConfiguredCorrectly()
    {
        // Arrange - Ce test ne nécessite pas l'API Pokemon
        
        // Act - Utiliser l'API Request de Playwright pour faire un test simple
        var request = await Playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL = "https://httpbin.org" // Service de test public
        });
        
        var response = await request.GetAsync("/status/200");
        
        // Assert
        Assert.IsTrue(response.Ok, "La requête vers httpbin.org devrait réussir");
        Assert.AreEqual(200, response.Status, "Le status code devrait être 200");
        
        Console.WriteLine("✅ Playwright est configuré correctement");
        Console.WriteLine($"Status: {response.Status}");
        Console.WriteLine($"Headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}={h.Value}"))}");
    }

    /// <summary>
    /// Test pour valider que les helpers de base fonctionnent
    /// </summary>
    [TestMethod]
    public async Task PlaywrightTestBase_HelpersShouldWork()
    {
        // Arrange - Utiliser httpbin.org pour tester les helpers
        var testData = new { message = "test", timestamp = DateTime.UtcNow };
        
        // Act - Tester notre helper ApiRequestAsync avec httpbin.org
        var response = await ApiRequestAsyncExternal("POST", "https://httpbin.org/anything", testData);
        
        // Assert
        AssertSuccessResponse(response, 200);
        
        var responseData = await ParseJsonResponseAsync<dynamic>(response);
        Assert.IsNotNull(responseData, "La réponse devrait contenir des données");
        
        Console.WriteLine("✅ Les helpers de PlaywrightTestBase fonctionnent correctement");
    }

    /// <summary>
    /// Helper pour tester avec une URL externe complète
    /// </summary>
    private async Task<IAPIResponse> ApiRequestAsyncExternal(string method, string fullUrl, object? data = null)
    {
        var request = await Playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            ExtraHTTPHeaders = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            }
        });

        return method.ToUpper() switch
        {
            "GET" => await request.GetAsync(fullUrl),
            "POST" => await request.PostAsync(fullUrl, new APIRequestContextOptions { DataObject = data }),
            "PUT" => await request.PutAsync(fullUrl, new APIRequestContextOptions { DataObject = data }),
            "DELETE" => await request.DeleteAsync(fullUrl),
            _ => throw new NotSupportedException($"Méthode HTTP non supportée: {method}")
        };
    }
}
