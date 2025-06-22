using Microsoft.Playwright;
using System.Text.Json;

namespace PokemonAPI.Tests.PlaywrightE2E
{
    /// <summary>
    /// Tests E2E pour l'authentification reproduisant les scénarios du fichier Auth.http
    /// Ces tests vérifient l'intégralité du flux d'authentification via l'API REST
    /// </summary>
    [TestClass]
    public class AuthenticationE2ETests : PlaywrightTestBase
    {
        private const string TestUsername = "testuser";
        private const string TestPassword = "Test@123";
        private const string TestEmail = "testuser@example.com";
        
        private const string AdminUsername = "admin";
        private const string AdminPassword = "admin123";

        [TestInitialize]
        public async Task Setup()
        {
            await base.TestInitialize();
            
            // Vérifier que l'API est accessible avant de commencer les tests
            var isHealthy = await IsApiHealthyAsync();
            if (!isHealthy)
            {
                Assert.Inconclusive("API is not accessible. Please ensure the Pokemon API is running on http://localhost:5183");
            }
        }

        /// <summary>
        /// Test d'enregistrement d'un nouvel utilisateur
        /// Correspond à la première requête POST /api/auth/register du fichier Auth.http
        /// </summary>
        [TestMethod]
        [TestCategory("Authentication")]
        [TestCategory("E2E")]
        public async Task Register_WithValidCredentials_ShouldSucceed()
        {
            // Arrange
            var registrationData = new
            {
                username = TestUsername,
                password = TestPassword,
                email = TestEmail
            };

            // Act
            var response = await MakeApiRequestAsync("POST", "/auth/register", registrationData);
            
            // Assert
            response.Status.Should().BeOneOf(200, 201, 409); // 409 si l'utilisateur existe déjà
            
            var responseText = await response.TextAsync();
            responseText.Should().NotBeNullOrEmpty();

            if (response.Status == 409)
            {
                // L'utilisateur existe déjà, c'est acceptable pour les tests
                Console.WriteLine("User already exists, registration test passed with expected conflict");
            }
            else
            {
                // Nouvelle inscription réussie
                response.Ok.Should().BeTrue("Registration should succeed with valid credentials");
                
                // Vérifier que la réponse contient les informations attendues
                var responseBody = await response.JsonAsync();
                responseBody.Should().NotBeNull("Response should contain JSON data");
            }
        }

        /// <summary>
        /// Test de connexion avec des identifiants administrateur
        /// Correspond à la requête POST /api/auth/login du fichier Auth.http
        /// </summary>
        [TestMethod]
        [TestCategory("Authentication")]
        [TestCategory("E2E")]
        public async Task Login_WithAdminCredentials_ShouldReturnToken()
        {
            // Arrange
            var loginData = new
            {
                username = AdminUsername,
                password = AdminPassword
            };

            // Act
            var response = await MakeApiRequestAsync("POST", "/auth/login", loginData);
            
            // Assert
            response.Ok.Should().BeTrue($"Login should succeed with admin credentials, but got status {response.Status}");
            
            var responseBody = await response.JsonAsync();
            responseBody.Should().NotBeNull("Response should contain JSON data");
            
            // Vérifier la présence du token
            var tokenProperty = responseBody?.TryGetProperty("token", out var tokenElement);
            tokenProperty.Should().BeTrue("Response should contain a token property");
            
            var token = tokenElement.GetString();
            token.Should().NotBeNullOrEmpty("Token should not be empty");
            token.Should().StartWith("eyJ", "Token should be a valid JWT token");
            
            Console.WriteLine($"Successfully obtained token: {token?[..20]}...");
        }

        /// <summary>
        /// Test de connexion avec des identifiants utilisateur test
        /// </summary>
        [TestMethod]
        [TestCategory("Authentication")]
        [TestCategory("E2E")]
        public async Task Login_WithTestUserCredentials_ShouldReturnToken()
        {
            // Arrange - S'assurer que l'utilisateur test existe
            await Register_WithValidCredentials_ShouldSucceed();
            
            var loginData = new
            {
                username = TestUsername,
                password = TestPassword
            };

            // Act
            var response = await MakeApiRequestAsync("POST", "/auth/login", loginData);
            
            // Assert
            response.Ok.Should().BeTrue($"Login should succeed with test user credentials, but got status {response.Status}");
            
            var responseBody = await response.JsonAsync();
            var token = responseBody?.GetProperty("token").GetString();
            token.Should().NotBeNullOrEmpty("Token should be returned for test user");
        }

        /// <summary>
        /// Test de validation du token d'authentification
        /// Correspond à la requête GET /api/auth/validate du fichier Auth.http
        /// </summary>
        [TestMethod]
        [TestCategory("Authentication")]
        [TestCategory("E2E")]
        public async Task ValidateAuth_WithValidToken_ShouldSucceed()
        {
            // Arrange - Obtenir un token valide
            var token = await AuthenticateAndGetTokenAsync(AdminUsername, AdminPassword);
            var authHeaders = CreateAuthHeaders(token);

            // Act
            var response = await MakeApiRequestAsync("GET", "/auth/validate", headers: authHeaders);
            
            // Assert
            response.Ok.Should().BeTrue($"Token validation should succeed, but got status {response.Status}");
            
            var responseBody = await response.JsonAsync();
            responseBody.Should().NotBeNull("Validation response should contain data");
            
            // Vérifier que la réponse contient des informations sur l'utilisateur
            var hasUserInfo = responseBody?.TryGetProperty("username", out _) == true ||
                            responseBody?.TryGetProperty("user", out _) == true ||
                            responseBody?.TryGetProperty("valid", out _) == true;
            
            hasUserInfo.Should().BeTrue("Validation response should contain user information or validation status");
        }

        /// <summary>
        /// Test d'accès à une ressource protégée avec authentification
        /// Correspond à la requête GET /api/Trainer avec Authorization du fichier Auth.http
        /// </summary>
        [TestMethod]
        [TestCategory("Authentication")]
        [TestCategory("E2E")]
        public async Task AccessProtectedResource_WithValidToken_ShouldSucceed()
        {
            // Arrange - Obtenir un token valide
            var token = await AuthenticateAndGetTokenAsync(AdminUsername, AdminPassword);
            var authHeaders = CreateAuthHeaders(token);

            // Act - Accéder à la ressource Trainer qui nécessite une authentification
            var response = await MakeApiRequestAsync("GET", "/Trainer", headers: authHeaders);
            
            // Assert
            response.Ok.Should().BeTrue($"Access to protected resource should succeed with valid token, but got status {response.Status}");
            
            var responseBody = await response.TextAsync();
            responseBody.Should().NotBeNullOrEmpty("Protected resource should return data");
            
            // Vérifier que la réponse contient des données JSON ou de la data
            if (response.Headers["content-type"]?.Contains("application/json") == true)
            {
                var jsonResponse = await response.JsonAsync();
                jsonResponse.Should().NotBeNull("JSON response should be valid");
            }
        }

        /// <summary>
        /// Test d'accès à une ressource protégée sans authentification
        /// </summary>
        [TestMethod]
        [TestCategory("Authentication")]
        [TestCategory("E2E")]
        public async Task AccessProtectedResource_WithoutToken_ShouldFail()
        {
            // Act - Tenter d'accéder à la ressource sans token
            var response = await MakeApiRequestAsync("GET", "/Trainer");
              // Assert
            response.Status.Should().BeOneOf(401, 403, 404); 
            // "Access to protected resource without token should return 401 Unauthorized, 403 Forbidden, or 404 if auth middleware redirects"
        }

        /// <summary>
        /// Test d'accès avec un token invalide
        /// </summary>
        [TestMethod]
        [TestCategory("Authentication")]
        [TestCategory("E2E")]
        public async Task AccessProtectedResource_WithInvalidToken_ShouldFail()
        {
            // Arrange - Créer un token invalide
            var invalidToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
            var authHeaders = CreateAuthHeaders(invalidToken);

            // Act
            var response = await MakeApiRequestAsync("GET", "/Trainer", headers: authHeaders);
              // Assert
            response.Status.Should().BeOneOf(401, 403);
            // "Access with invalid token should return 401 Unauthorized or 403 Forbidden"
        }

        /// <summary>
        /// Test de connexion avec des identifiants invalides
        /// </summary>
        [TestMethod]
        [TestCategory("Authentication")]
        [TestCategory("E2E")]
        public async Task Login_WithInvalidCredentials_ShouldFail()
        {
            // Arrange
            var invalidLoginData = new
            {
                username = "nonexistentuser",
                password = "wrongpassword"
            };

            // Act
            var response = await MakeApiRequestAsync("POST", "/auth/login", invalidLoginData);
              // Assert
            response.Status.Should().BeOneOf(400, 401, 404);
            // "Login with invalid credentials should fail with 400, 401, or 404"
        }

        /// <summary>
        /// Test de scénario complet d'authentification
        /// Reproduit l'ensemble du workflow du fichier Auth.http
        /// </summary>
        [TestMethod]
        [TestCategory("Authentication")]
        [TestCategory("E2E")]
        [TestCategory("Integration")]
        public async Task CompleteAuthFlow_RegisterLoginValidateAccess_ShouldSucceed()
        {
            // Étape 1: Enregistrement
            await Register_WithValidCredentials_ShouldSucceed();
            
            // Étape 2: Connexion et récupération du token
            var token = await AuthenticateAndGetTokenAsync(TestUsername, TestPassword);
            
            // Étape 3: Validation du token
            var authHeaders = CreateAuthHeaders(token);
            var validateResponse = await MakeApiRequestAsync("GET", "/auth/validate", headers: authHeaders);
            validateResponse.Ok.Should().BeTrue("Token validation should succeed");
            
            // Étape 4: Accès à une ressource protégée
            var trainerResponse = await MakeApiRequestAsync("GET", "/Trainer", headers: authHeaders);
            trainerResponse.Ok.Should().BeTrue("Access to protected resource should succeed");
            
            Console.WriteLine("Complete authentication flow succeeded: Register -> Login -> Validate -> Access Protected Resource");
        }

        /// <summary>
        /// Test de performance pour l'authentification
        /// </summary>
        [TestMethod]
        [TestCategory("Authentication")]
        [TestCategory("E2E")]
        [TestCategory("Performance")]
        public async Task Authentication_ShouldRespondQuickly()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Mesurer le temps de connexion
            var token = await AuthenticateAndGetTokenAsync(AdminUsername, AdminPassword);
            
            stopwatch.Stop();
            var loginTime = stopwatch.ElapsedMilliseconds;
            
            // Reset pour mesurer la validation
            stopwatch.Restart();
            var authHeaders = CreateAuthHeaders(token);
            var response = await MakeApiRequestAsync("GET", "/auth/validate", headers: authHeaders);
            stopwatch.Stop();
            var validateTime = stopwatch.ElapsedMilliseconds;
            
            // Assertions de performance
            loginTime.Should().BeLessThan(5000, "Login should complete within 5 seconds");
            validateTime.Should().BeLessThan(2000, "Token validation should complete within 2 seconds");
            response.Ok.Should().BeTrue("Validation should succeed");
            
            Console.WriteLine($"Performance metrics - Login: {loginTime}ms, Validation: {validateTime}ms");
        }
    }
}
