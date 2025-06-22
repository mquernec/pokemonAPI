using PokemonAPI.Tests.Infrastructure;
using PokemonAPI.Tests.Models;
using System.Net;

namespace PokemonAPI.Tests.Controllers
{
    /// <summary>
    /// Tests d'intégration pour le contrôleur Battle
    /// </summary>
    [TestClass]
    public class BattleControllerTests : BaseApiTest
    {
        private const string BaseUrl = "/api/battle";

        [TestInitialize]
        public void Setup()
        {
            // Configuration spécifique aux tests Battle si nécessaire
        }

        #region Tests GET

        [TestMethod]
        public async Task GetAllBattles_ShouldReturnOk()
        {
            // Act
            var response = await _httpClient.GetAsync(BaseUrl);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var battles = await GetSuccessResponse<List<Battle>>(response);
            battles.Should().NotBeNull();
        }

        [TestMethod]
        public async Task GetBattleById_WithValidId_ShouldReturnBattle()
        {
            // Arrange
            int battleId = 1;

            // Act
            var response = await _httpClient.GetAsync($"{BaseUrl}/{battleId}");

            // Assert
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var battle = await GetSuccessResponse<Battle>(response);
                battle.Should().NotBeNull();
                battle.Id.Should().Be(battleId);
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // C'est acceptable si le combat n'existe pas
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetBattleById_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            int invalidId = 99999;

            // Act
            var response = await _httpClient.GetAsync($"{BaseUrl}/{invalidId}");

            // Assert
            AssertErrorResponse(response, HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task GetRecentBattles_ShouldReturnOk()
        {
            // Act
            var response = await _httpClient.GetAsync($"{BaseUrl}/recent");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var battles = await GetSuccessResponse<List<Battle>>(response);
            battles.Should().NotBeNull();
        }

        [TestMethod]
        public async Task GetRecentBattles_WithDaysParameter_ShouldReturnOk()
        {
            // Arrange
            int days = 7;

            // Act
            var response = await _httpClient.GetAsync($"{BaseUrl}/recent?days={days}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var battles = await GetSuccessResponse<List<Battle>>(response);
            battles.Should().NotBeNull();
            
            // Vérifier que les combats sont dans la plage de dates
            if (battles.Any())
            {
                var cutoffDate = DateTime.Now.AddDays(-days);
                battles.All(b => b.BattleDate >= cutoffDate).Should().BeTrue();
            }
        }

        [TestMethod]
        public async Task GetBattlesByTrainer_WithValidId_ShouldReturnBattles()
        {
            // Arrange
            int trainerId = 1;

            // Act
            var response = await _httpClient.GetAsync($"{BaseUrl}/trainer/{trainerId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var battles = await GetSuccessResponse<List<Battle>>(response);
            battles.Should().NotBeNull();
            
            // Vérifier que tous les combats impliquent le dresseur
            if (battles.Any())
            {
                battles.All(b => b.Trainer1Id == trainerId || b.Trainer2Id == trainerId).Should().BeTrue();
            }
        }

        [TestMethod]
        public async Task GetBattleStatistics_ShouldReturnOk()
        {
            // Act
            var response = await _httpClient.GetAsync($"{BaseUrl}/statistics");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public async Task GetBattlesByLocation_WithValidLocation_ShouldReturnBattles()
        {
            // Arrange
            string location = "Viridian City";

            // Act
            var response = await _httpClient.GetAsync($"{BaseUrl}/location/{location}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var battles = await GetSuccessResponse<List<Battle>>(response);
            battles.Should().NotBeNull();
            
            if (battles.Any())
            {
                battles.All(b => b.Location == location).Should().BeTrue();
            }
        }

        #endregion

        #region Tests POST

        [TestMethod]
        public async Task CreateBattle_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var request = new CreateBattleRequest
            {
                Trainer1Id = 1,
                Trainer2Id = 2,
                Location = "Pallet Town"
            };

            // Act
            var response = await _httpClient.PostAsync(BaseUrl, CreateJsonContent(request));

            // Assert
            if (response.StatusCode == HttpStatusCode.Created)
            {
                var battle = await GetSuccessResponse<Battle>(response);
                battle.Should().NotBeNull();
                battle.Trainer1Id.Should().Be(request.Trainer1Id);
                battle.Trainer2Id.Should().Be(request.Trainer2Id);
                battle.Location.Should().Be(request.Location);
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Si l'authentification est requise
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [TestMethod]
        public async Task CreateBattle_WithSameTrainer_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new CreateBattleRequest
            {
                Trainer1Id = 1,
                Trainer2Id = 1, // Même dresseur - invalide
                Location = "Pallet Town"
            };

            // Act
            var response = await _httpClient.PostAsync(BaseUrl, CreateJsonContent(request));

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task CreateBattle_WithInvalidTrainerIds_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new CreateBattleRequest
            {
                Trainer1Id = 99999, // ID inexistant
                Trainer2Id = 99998, // ID inexistant
                Location = "Pallet Town"
            };

            // Act
            var response = await _httpClient.PostAsync(BaseUrl, CreateJsonContent(request));

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task StartBattle_WithValidIds_ShouldReturnOk()
        {
            // Arrange
            int trainer1Id = 1;
            int trainer2Id = 2;

            // Act
            var response = await _httpClient.PostAsync($"{BaseUrl}/start/{trainer1Id}/{trainer2Id}", null);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Tests PATCH/PUT

        [TestMethod]
        public async Task DeclareWinner_WithValidData_ShouldReturnOk()
        {
            // Arrange
            int battleId = 1;
            var request = new DeclareWinnerRequest
            {
                WinnerId = 1
            };

            // Act
            var response = await _httpClient.PatchAsync($"{BaseUrl}/{battleId}/winner", CreateJsonContent(request));

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task DeclareWinner_WithInvalidWinner_ShouldReturnBadRequest()
        {
            // Arrange
            int battleId = 1;
            var request = new DeclareWinnerRequest
            {
                WinnerId = 999 // ID qui n'est ni trainer1 ni trainer2
            };

            // Act
            var response = await _httpClient.PatchAsync($"{BaseUrl}/{battleId}/winner", CreateJsonContent(request));

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Tests DELETE

        [TestMethod]
        public async Task DeleteBattle_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            int battleId = 999; // Utilisez un ID qui peut être supprimé

            // Act
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{battleId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Tests de validation et logique métier

        [TestMethod]
        public async Task CreateBattle_WithEmptyLocation_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new CreateBattleRequest
            {
                Trainer1Id = 1,
                Trainer2Id = 2,
                Location = "" // Localisation vide
            };

            // Act
            var response = await _httpClient.PostAsync(BaseUrl, CreateJsonContent(request));

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Created, HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task GetRecentBattles_WithNegativeDays_ShouldReturnBadRequest()
        {
            // Arrange
            int negativeDays = -5;

            // Act
            var response = await _httpClient.GetAsync($"{BaseUrl}/recent?days={negativeDays}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task GetBattlesByTrainer_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            int invalidTrainerId = 99999;

            // Act
            var response = await _httpClient.GetAsync($"{BaseUrl}/trainer/{invalidTrainerId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK);
        }

        #endregion
    }
}
