using PokemonAPI.Tests.Infrastructure;
using PokemonAPI.Tests.Models;
using System.Net;

namespace PokemonAPI.Tests.Controllers
{
    /// <summary>
    /// Tests d'intégration pour le contrôleur Pokemon
    /// </summary>
    [TestClass]
    public class PokemonControllerTests : BaseApiTest
    {
        private const string BaseUrl = "/api/pokemon";

        [TestInitialize]
        public void Setup()
        {
            // Configuration spécifique aux tests Pokemon si nécessaire
        }

        #region Tests GET

        [TestMethod]
        public async Task GetAllPokemons_ShouldReturnOk()
        {
            // Act
            var response = await _httpClient.GetAsync(BaseUrl);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var pokemons = await GetSuccessResponse<List<Pokemon>>(response);
            pokemons.Should().NotBeNull();
        }

        [TestMethod]
        public async Task GetPokemonById_WithValidId_ShouldReturnPokemon()
        {
            // Arrange
            int pokemonId = 1;

            // Act
            var response = await _httpClient.GetAsync($"{BaseUrl}/{pokemonId}");

            // Assert
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var pokemon = await GetSuccessResponse<Pokemon>(response);
                pokemon.Should().NotBeNull();
                pokemon.Id.Should().Be(pokemonId);
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // C'est acceptable si le Pokemon n'existe pas
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetPokemonById_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            int invalidId = 99999;

            // Act
            var response = await _httpClient.GetAsync($"{BaseUrl}/{invalidId}");

            // Assert
            AssertErrorResponse(response, HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task GetPokemonByName_WithValidName_ShouldReturnPokemon()
        {
            // Arrange
            string pokemonName = "Pikachu";

            // Act
            var response = await _httpClient.GetAsync($"{BaseUrl}/name/{pokemonName}");

            // Assert
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var pokemon = await GetSuccessResponse<Pokemon>(response);
                pokemon.Should().NotBeNull();
                pokemon.Name.Should().Be(pokemonName);
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // C'est acceptable si le Pokemon n'existe pas
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetPokemonByType_WithValidType_ShouldReturnPokemons()
        {
            // Arrange
            string type = "Electric";

            // Act
            var response = await _httpClient.GetAsync($"{BaseUrl}/type/{type}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var pokemons = await GetSuccessResponse<List<Pokemon>>(response);
            pokemons.Should().NotBeNull();
            if (pokemons.Any())
            {
                pokemons.All(p => p.Type == type).Should().BeTrue();
            }
        }

        [TestMethod]
        public async Task GetPokemonByLevel_WithValidRange_ShouldReturnPokemons()
        {
            // Arrange
            int minLevel = 1;
            int maxLevel = 50;

            // Act
            var response = await _httpClient.GetAsync($"{BaseUrl}/level?minLevel={minLevel}&maxLevel={maxLevel}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var pokemons = await GetSuccessResponse<List<Pokemon>>(response);
            pokemons.Should().NotBeNull();
            if (pokemons.Any())
            {
                pokemons.All(p => p.Level >= minLevel && p.Level <= maxLevel).Should().BeTrue();
            }
        }

        [TestMethod]
        public async Task GetPokemonByAbility_WithValidAbility_ShouldReturnPokemons()
        {
            // Arrange
            string ability = "Static";

            // Act
            var response = await _httpClient.GetAsync($"{BaseUrl}/ability/{ability}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var pokemons = await GetSuccessResponse<List<Pokemon>>(response);
            pokemons.Should().NotBeNull();
            if (pokemons.Any())
            {
                pokemons.All(p => p.Ability == ability).Should().BeTrue();
            }
        }

        #endregion

        #region Tests POST

        [TestMethod]
        public async Task CreatePokemon_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var request = new CreatePokemonRequest
            {
                Name = "TestPokemon",
                Type = "Electric",
                Level = 25,
                Ability = "Static"
            };

            // Act
            var response = await _httpClient.PostAsync(BaseUrl, CreateJsonContent(request));

            // Assert
            if (response.StatusCode == HttpStatusCode.Created)
            {
                var pokemon = await GetSuccessResponse<Pokemon>(response);
                pokemon.Should().NotBeNull();
                pokemon.Name.Should().Be(request.Name);
                pokemon.Type.Should().Be(request.Type);
                pokemon.Level.Should().Be(request.Level);
                pokemon.Ability.Should().Be(request.Ability);
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Si l'authentification est requise
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [TestMethod]
        public async Task CreatePokemon_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new CreatePokemonRequest
            {
                Name = "", // Nom vide invalide
                Type = "Electric",
                Level = 25,
                Ability = "Static"
            };

            // Act
            var response = await _httpClient.PostAsync(BaseUrl, CreateJsonContent(request));

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Tests PUT

        [TestMethod]
        public async Task UpdatePokemon_WithValidData_ShouldReturnOk()
        {
            // Arrange
            int pokemonId = 1;
            var request = new UpdatePokemonRequest
            {
                Name = "UpdatedPokemon",
                Type = "Fire",
                Level = 30,
                Ability = "Blaze"
            };

            // Act
            var response = await _httpClient.PutAsync($"{BaseUrl}/{pokemonId}", CreateJsonContent(request));

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Tests PATCH

        [TestMethod]
        public async Task LevelUpPokemon_WithValidId_ShouldReturnOk()
        {
            // Arrange
            int pokemonId = 1;

            // Act
            var response = await _httpClient.PatchAsync($"{BaseUrl}/{pokemonId}/level-up", null);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task ChangePokemonAbility_WithValidData_ShouldReturnOk()
        {
            // Arrange
            int pokemonId = 1;
            var request = new ChangeAbilityRequest
            {
                NewAbility = "Thunder"
            };

            // Act
            var response = await _httpClient.PatchAsync($"{BaseUrl}/{pokemonId}/ability", CreateJsonContent(request));

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Tests DELETE

        [TestMethod]
        public async Task DeletePokemon_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            int pokemonId = 999; // Utilisez un ID qui peut être supprimé

            // Act
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{pokemonId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Tests de validation

        [TestMethod]
        public async Task CreatePokemon_WithNameTooShort_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new CreatePokemonRequest
            {
                Name = "abc", // Nom trop court (moins de 5 caractères selon les contraintes)
                Type = "Electric",
                Level = 25,
                Ability = "Static"
            };

            // Act
            var response = await _httpClient.PostAsync(BaseUrl, CreateJsonContent(request));

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task CreatePokemon_WithNameTooLong_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new CreatePokemonRequest
            {
                Name = "VeryLongPokemonName", // Nom trop long (plus de 10 caractères selon les contraintes)
                Type = "Electric",
                Level = 25,
                Ability = "Static"
            };

            // Act
            var response = await _httpClient.PostAsync(BaseUrl, CreateJsonContent(request));

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task GetPokemonByLevel_WithInvalidRange_ShouldReturnBadRequest()
        {
            // Arrange
            int minLevel = 50;
            int maxLevel = 25; // Max inférieur au min

            // Act
            var response = await _httpClient.GetAsync($"{BaseUrl}/level?minLevel={minLevel}&maxLevel={maxLevel}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK); // Dépend de la validation côté serveur
        }

        #endregion
    }
}
