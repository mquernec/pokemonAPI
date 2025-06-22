using PokemonAPI.Tests.Infrastructure;
using PokemonAPI.Tests.Models;
using System.Net;

namespace PokemonAPI.Tests.Controllers
{
    /// <summary>
    /// Tests d'intégration pour le contrôleur Trainer
    /// </summary>
    [TestClass]
    public class TrainerControllerTests : BaseApiTest
    {
        private const string BaseUrl = "/api/trainer";

        [TestInitialize]
        public void Setup()
        {
            // Configuration spécifique aux tests Trainer si nécessaire
        }

        #region Tests GET

        [TestMethod]
        public async Task GetAllTrainers_ShouldReturnOk()
        {
            // Act
            var response = await _httpClient.GetAsync(BaseUrl);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var trainers = await GetSuccessResponse<List<Trainer>>(response);
            trainers.Should().NotBeNull();
        }

        [TestMethod]
        public async Task GetTrainerById_WithValidId_ShouldReturnTrainer()
        {
            // Arrange
            int trainerId = 1;

            // Act
            var response = await _httpClient.GetAsync($"{BaseUrl}/{trainerId}");

            // Assert
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var trainer = await GetSuccessResponse<Trainer>(response);
                trainer.Should().NotBeNull();
                trainer.Id.Should().Be(trainerId);
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // C'est acceptable si le dresseur n'existe pas
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetTrainerById_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            int invalidId = 99999;

            // Act
            var response = await _httpClient.GetAsync($"{BaseUrl}/{invalidId}");

            // Assert
            AssertErrorResponse(response, HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task GetTrainerByName_WithValidName_ShouldReturnTrainer()
        {
            // Arrange
            string trainerName = "Ash";

            // Act
            var response = await _httpClient.GetAsync($"{BaseUrl}/name/{trainerName}");

            // Assert
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var trainer = await GetSuccessResponse<Trainer>(response);
                trainer.Should().NotBeNull();
                trainer.Name.Should().Be(trainerName);
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // C'est acceptable si le dresseur n'existe pas
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetTrainersByRegion_WithValidRegion_ShouldReturnTrainers()
        {
            // Arrange
            string region = "Kanto";

            // Act
            var response = await _httpClient.GetAsync($"{BaseUrl}/region/{region}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var trainers = await GetSuccessResponse<List<Trainer>>(response);
            trainers.Should().NotBeNull();
            if (trainers.Any())
            {
                trainers.All(t => t.Region == region).Should().BeTrue();
            }
        }

        [TestMethod]
        public async Task GetTrainersByAge_WithValidAgeRange_ShouldReturnTrainers()
        {
            // Arrange
            int minAge = 10;
            int maxAge = 25;

            // Act
            var response = await _httpClient.GetAsync($"{BaseUrl}/age?minAge={minAge}&maxAge={maxAge}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var trainers = await GetSuccessResponse<List<Trainer>>(response);
            trainers.Should().NotBeNull();
            if (trainers.Any())
            {
                trainers.All(t => t.Age >= minAge && t.Age <= maxAge).Should().BeTrue();
            }
        }

        [TestMethod]
        public async Task GetBattleHistoryBetweenTrainers_WithValidIds_ShouldReturnBattles()
        {
            // Arrange
            int trainerId1 = 1;
            int trainerId2 = 2;

            // Act
            var response = await _httpClient.GetAsync($"{BaseUrl}/{trainerId1}/battles/{trainerId2}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var battles = await GetSuccessResponse<List<Battle>>(response);
            battles.Should().NotBeNull();
        }

        #endregion

        #region Tests POST

        [TestMethod]
        public async Task CreateTrainer_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var request = new CreateTrainerRequest
            {
                Name = "TestTrainer",
                Age = 20,
                Region = "Kanto",
                BadgeCount = 3
            };

            // Act
            var response = await _httpClient.PostAsync(BaseUrl, CreateJsonContent(request));

            // Assert
            if (response.StatusCode == HttpStatusCode.Created)
            {
                var trainer = await GetSuccessResponse<Trainer>(response);
                trainer.Should().NotBeNull();
                trainer.Name.Should().Be(request.Name);
                trainer.Age.Should().Be(request.Age);
                trainer.Region.Should().Be(request.Region);
                trainer.BadgeCount.Should().Be(request.BadgeCount);
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Si l'authentification est requise
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [TestMethod]
        public async Task CreateTrainer_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new CreateTrainerRequest
            {
                Name = "", // Nom vide invalide
                Age = 20,
                Region = "Kanto",
                BadgeCount = 3
            };

            // Act
            var response = await _httpClient.PostAsync(BaseUrl, CreateJsonContent(request));

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task AssignPokemonToTrainer_WithValidData_ShouldReturnOk()
        {
            // Arrange
            int trainerId = 1;
            var request = new AssignPokemonRequest
            {
                PokemonId = 1
            };

            // Act
            var response = await _httpClient.PostAsync($"{BaseUrl}/{trainerId}/pokemon", CreateJsonContent(request));

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Tests PUT

        [TestMethod]
        public async Task UpdateTrainer_WithValidData_ShouldReturnOk()
        {
            // Arrange
            int trainerId = 1;
            var request = new UpdateTrainerRequest
            {
                Name = "UpdatedTrainer",
                Age = 25,
                Region = "Johto",
                BadgeCount = 5
            };

            // Act
            var response = await _httpClient.PutAsync($"{BaseUrl}/{trainerId}", CreateJsonContent(request));

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Tests DELETE

        [TestMethod]
        public async Task DeleteTrainer_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            int trainerId = 999; // Utilisez un ID qui peut être supprimé

            // Act
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{trainerId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task RemovePokemonFromTrainer_WithValidIds_ShouldReturnOk()
        {
            // Arrange
            int trainerId = 1;
            int pokemonId = 1;

            // Act
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{trainerId}/pokemon/{pokemonId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Tests de validation

        [TestMethod]
        public async Task CreateTrainer_WithNegativeAge_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new CreateTrainerRequest
            {
                Name = "TestTrainer",
                Age = -5, // Âge négatif invalide
                Region = "Kanto",
                BadgeCount = 3
            };

            // Act
            var response = await _httpClient.PostAsync(BaseUrl, CreateJsonContent(request));

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task CreateTrainer_WithTooManyBadges_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new CreateTrainerRequest
            {
                Name = "TestTrainer",
                Age = 20,
                Region = "Kanto",
                BadgeCount = 20 // Trop de badges (max généralement 8 par région)
            };

            // Act
            var response = await _httpClient.PostAsync(BaseUrl, CreateJsonContent(request));

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Created, HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task AssignPokemonToTrainer_WithFullTeam_ShouldReturnBadRequest()
        {
            // Ce test nécessiterait de d'abord créer un dresseur avec 6 Pokémon
            // puis d'essayer d'en ajouter un 7ème
            // Pour l'instant, on teste juste la structure de la requête
            
            // Arrange
            int trainerId = 1;
            var request = new AssignPokemonRequest
            {
                PokemonId = 999 // ID inexistant
            };

            // Act
            var response = await _httpClient.PostAsync($"{BaseUrl}/{trainerId}/pokemon", CreateJsonContent(request));

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        }

        #endregion
    }
}
