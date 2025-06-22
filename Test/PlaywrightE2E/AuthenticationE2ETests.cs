using Microsoft.Playwright;
using System.Text.Json;

namespace PokemonAPI.Tests.PlaywrightE2E;

/// <summary>
/// Tests end-to-end pour l'authentification API basés sur Auth.http
/// </summary>
[TestClass]
public class AuthenticationE2ETests : PlaywrightTestBase
{
    private const string ValidUsername = "admin";
    private const string ValidPassword = "admin123";
    private const string TestUserEmail = "testuser@example.com";
    private const string TestUsername = "testuser";
    private const string TestPassword = "Test@123";

    /// <summary>
    /// Modèle pour les réponses d'authentification
    /// </summary>
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// Modèle pour les réponses de validation
    /// </summary>
    public class ValidationResponse
    {
        public string Message { get; set; } = string.Empty;
        public UserInfo User { get; set; } = new();
    }

    public class UserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

  
    /// <summary>
    /// Test: POST /api/auth/login - Connexion avec des identifiants valides
    /// Reproduit la deuxième requête du fichier Auth.http
    /// </summary>
    [TestMethod]
    public async Task Login_WithValidCredentials_ShouldReturnTokenAndUserInfo()
    {
        // Arrange
        var loginData = new
        {
            username = ValidUsername,
            password = ValidPassword
        };

        // Act
        var response = await ApiRequestAsync("POST", "/api/auth/login", loginData);

        // Assert
        AssertSuccessResponse(response);

        var authResponse = await ParseJsonResponseAsync<AuthResponse>(response);
        Assert.IsNotNull(authResponse);
        Assert.IsFalse(string.IsNullOrEmpty(authResponse.Token), "Le token ne devrait pas être vide");


        Assert.IsTrue(authResponse.ExpiresAt > DateTime.UtcNow, "Le token devrait avoir une date d'expiration future");

        // Vérification supplémentaire: le token devrait commencer par le format JWT
        Assert.IsTrue(authResponse.Token.Split('.').Length == 3, "Le token devrait être au format JWT (3 parties séparées par des points)");
    }

    /// <summary>
    /// Test: POST /api/auth/login - Connexion avec des identifiants invalides
    /// </summary>
    [TestMethod]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginData = new
        {
            username = "wronguser",
            password = "wrongpassword"
        };

        // Act
        var response = await ApiRequestAsync("POST", "/api/auth/login", loginData);

        // Assert
        AssertErrorResponse(response, 401);
    }

 
    

    /// <summary>
    /// Test: GET /api/auth/validate - Validation sans token
    /// </summary>
    [TestMethod]
    public async Task ValidateToken_WithoutToken_ShouldReturnUnauthorized()
    {
        // Act
        var response = await ApiRequestAsync("GET", "/api/auth/validate");

        // Assert
        AssertErrorResponse(response, 401);
    }

    /// <summary>
    /// Test: GET /api/auth/validate - Validation avec un token invalide
    /// </summary>
    [TestMethod]
    public async Task ValidateToken_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var headers = new Dictionary<string, string>
        {
            { "Authorization", "Bearer invalid_token_here" }
        };

        // Act
        var response = await ApiRequestAsync("GET", "/api/auth/validate", null, headers);

        // Assert
        AssertErrorResponse(response, 401);
    }

    /// <summary>
    /// Test: GET /api/Trainer - Accès à une ressource protégée avec token valide
    /// Reproduit la quatrième requête du fichier Auth.http
    /// </summary>
    [TestMethod]
    public async Task AccessProtectedResource_WithValidToken_ShouldReturnData()
    {
        // Arrange: D'abord se connecter pour obtenir un token
        var loginData = new
        {
            username = ValidUsername,
            password = ValidPassword
        };

        var loginResponse = await ApiRequestAsync("POST", "/api/auth/login", loginData);
        AssertSuccessResponse(loginResponse);

        var authResponse = await ParseJsonResponseAsync<AuthResponse>(loginResponse);
        Assert.IsNotNull(authResponse);
        var token = authResponse.Token;

        // Act: Accéder à la ressource protégée
        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {token}" }
        };

        var trainerResponse = await ApiRequestAsync("GET", "/api/Trainer", null, headers);

        // Assert
        AssertSuccessResponse(trainerResponse);

        // Vérifier que la réponse contient des données (format dépend de l'implémentation de l'API)
        var responseContent = await trainerResponse.TextAsync();
        Assert.IsFalse(string.IsNullOrEmpty(responseContent), "La réponse ne devrait pas être vide");
    }

    /// <summary>
    /// Test: GET /api/Trainer - Accès à une ressource protégée sans token
    /// </summary>
    [TestMethod]
    public async Task AccessProtectedResource_WithoutToken_ShouldReturnUnauthorized()
    {
        // Act
        var response = await ApiRequestAsync("GET", "/api/Trainer");

        // Assert
        AssertErrorResponse(response, 401);
    }

}
