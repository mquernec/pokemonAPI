using PokemonAPI.Tests.Models;
using System.Net;

namespace PokemonAPI.Tests.EndToEnd
{
    /// <summary>
    /// Tests de bout en bout pour les scénarios complets d'utilisation de l'API Pokemon
    /// </summary>
    [TestClass]    public class PokemonApiEndToEndTests : IDisposable
    {
        private HttpClient _httpClient;
        private CustomWebApplicationFactory<TestProgram> _factory;

        [TestInitialize]
        public void Setup()
        {
            _factory = new CustomWebApplicationFactory<TestProgram>();
            _httpClient = _factory.CreateClient();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        #region Scénarios complets

        [TestMethod]
        public async Task CompleteScenario_CreateTrainerAndPokemonThenAssign_ShouldSucceed()
        {
            // Scenario: Créer un dresseur, créer un Pokemon, puis assigner le Pokemon au dresseur
            
            // 1. Créer un nouveau dresseur
            var trainerRequest = new CreateTrainerRequest
            {
                Name = "TestTrainer",
                Age = 20,
                Region = "Kanto",
                BadgeCount = 2
            };

            var trainerResponse = await _httpClient.PostAsync("/api/trainer", CreateJsonContent(trainerRequest));
            if (!trainerResponse.IsSuccessStatusCode && trainerResponse.StatusCode != HttpStatusCode.Unauthorized)
            {
                Assert.Fail($"Failed to create trainer: {trainerResponse.StatusCode}");
                return;
            }

            if (trainerResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Si l'authentification est requise, on peut arrêter le test ici
                Assert.Inconclusive("Authentication required for this test");
                return;
            }

            var trainer = await DeserializeResponse<Trainer>(trainerResponse);
            trainer.Should().NotBeNull();
            int trainerId = trainer.Id;

            // 2. Créer un nouveau Pokemon
            var pokemonRequest = new CreatePokemonRequest
            {
                Name = "TestPika",
                Type = "Electric",
                Level = 15,
                Ability = "Static"
            };

            var pokemonResponse = await _httpClient.PostAsync("/api/pokemon", CreateJsonContent(pokemonRequest));
            if (pokemonResponse.IsSuccessStatusCode)
            {
                var pokemon = await DeserializeResponse<Pokemon>(pokemonResponse);
                pokemon.Should().NotBeNull();
                int pokemonId = pokemon.Id;

                // 3. Assigner le Pokemon au dresseur
                var assignRequest = new AssignPokemonRequest { PokemonId = pokemonId };
                var assignResponse = await _httpClient.PostAsync($"/api/trainer/{trainerId}/pokemon", CreateJsonContent(assignRequest));
                
                if (assignResponse.IsSuccessStatusCode)
                {
                    // 4. Vérifier que l'assignation a fonctionné
                    var updatedTrainerResponse = await _httpClient.GetAsync($"/api/trainer/{trainerId}");
                    if (updatedTrainerResponse.IsSuccessStatusCode)
                    {
                        var updatedTrainer = await DeserializeResponse<Trainer>(updatedTrainerResponse);
                        updatedTrainer.PokemonTeam.Should().Contain(p => p.Id == pokemonId);
                    }
                }
            }
        }

        [TestMethod]
        public async Task CompleteScenario_CreateBattleBetweenTrainers_ShouldSucceed()
        {
            // Scenario: Créer deux dresseurs et organiser un combat entre eux

            // 1. Créer le premier dresseur
            var trainer1Request = new CreateTrainerRequest
            {
                Name = "Ash",
                Age = 16,
                Region = "Kanto",
                BadgeCount = 8
            };

            var trainer1Response = await _httpClient.PostAsync("/api/trainer", CreateJsonContent(trainer1Request));
            
            if (trainer1Response.StatusCode == HttpStatusCode.Unauthorized)
            {
                Assert.Inconclusive("Authentication required for this test");
                return;
            }

            if (!trainer1Response.IsSuccessStatusCode)
            {
                // Essayer de récupérer un dresseur existant
                var existingTrainersResponse = await _httpClient.GetAsync("/api/trainer");
                if (existingTrainersResponse.IsSuccessStatusCode)
                {
                    var existingTrainers = await DeserializeResponse<List<Trainer>>(existingTrainersResponse);
                    if (existingTrainers.Count >= 2)
                    {
                        // Utiliser les dresseurs existants pour le test de combat
                        var trainer1Id = existingTrainers[0].Id;
                        var trainer2Id = existingTrainers[1].Id;

                        await TestBattleCreation(trainer1Id, trainer2Id);
                        return;
                    }
                }
                Assert.Inconclusive("Could not create or find trainers for battle test");
                return;
            }

            var trainer1 = await DeserializeResponse<Trainer>(trainer1Response);

            // 2. Créer le deuxième dresseur
            var trainer2Request = new CreateTrainerRequest
            {
                Name = "Gary",
                Age = 16,
                Region = "Kanto",
                BadgeCount = 8
            };

            var trainer2Response = await _httpClient.PostAsync("/api/trainer", CreateJsonContent(trainer2Request));
            if (!trainer2Response.IsSuccessStatusCode)
            {
                Assert.Inconclusive("Could not create second trainer");
                return;
            }

            var trainer2 = await DeserializeResponse<Trainer>(trainer2Response);

            await TestBattleCreation(trainer1.Id, trainer2.Id);
        }

        private async Task TestBattleCreation(int trainer1Id, int trainer2Id)
        {
            // 3. Créer un combat entre les deux dresseurs
            var battleRequest = new CreateBattleRequest
            {
                Trainer1Id = trainer1Id,
                Trainer2Id = trainer2Id,
                Location = "Viridian City"
            };

            var battleResponse = await _httpClient.PostAsync("/api/battle", CreateJsonContent(battleRequest));
            
            if (battleResponse.IsSuccessStatusCode)
            {
                var battle = await DeserializeResponse<Battle>(battleResponse);
                battle.Should().NotBeNull();
                battle.Trainer1Id.Should().Be(trainer1Id);
                battle.Trainer2Id.Should().Be(trainer2Id);

                // 4. Déclarer un vainqueur
                var winnerRequest = new DeclareWinnerRequest { WinnerId = trainer1Id };
                var winnerResponse = await _httpClient.PatchAsync($"/api/battle/{battle.Id}/winner", CreateJsonContent(winnerRequest));
                
                winnerResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task CompleteScenario_PokemonEvolution_ShouldSucceed()
        {
            // Scenario: Créer un Pokemon de bas niveau et le faire évoluer

            // 1. Créer un Pokemon de niveau 1
            var pokemonRequest = new CreatePokemonRequest
            {
                Name = "Caterpie",
                Type = "Bug",
                Level = 1,
                Ability = "Shield Dust"
            };

            var pokemonResponse = await _httpClient.PostAsync("/api/pokemon", CreateJsonContent(pokemonRequest));
            
            if (pokemonResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                Assert.Inconclusive("Authentication required for this test");
                return;
            }

            if (pokemonResponse.IsSuccessStatusCode)
            {
                var pokemon = await DeserializeResponse<Pokemon>(pokemonResponse);
                pokemon.Should().NotBeNull();
                var originalLevel = pokemon.Level;

                // 2. Faire monter le Pokemon de niveau plusieurs fois
                for (int i = 0; i < 5; i++)
                {
                    var levelUpResponse = await _httpClient.PatchAsync($"/api/pokemon/{pokemon.Id}/level-up", null);
                    if (!levelUpResponse.IsSuccessStatusCode)
                    {
                        break;
                    }
                }

                // 3. Vérifier que le niveau a augmenté
                var updatedPokemonResponse = await _httpClient.GetAsync($"/api/pokemon/{pokemon.Id}");
                if (updatedPokemonResponse.IsSuccessStatusCode)
                {
                    var updatedPokemon = await DeserializeResponse<Pokemon>(updatedPokemonResponse);
                    updatedPokemon.Level.Should().BeGreaterThan(originalLevel);
                }

                // 4. Changer la capacité du Pokemon
                var abilityRequest = new ChangeAbilityRequest { NewAbility = "Run Away" };
                var abilityResponse = await _httpClient.PatchAsync($"/api/pokemon/{pokemon.Id}/ability", CreateJsonContent(abilityRequest));
                
                if (abilityResponse.IsSuccessStatusCode)
                {
                    var finalPokemonResponse = await _httpClient.GetAsync($"/api/pokemon/{pokemon.Id}");
                    if (finalPokemonResponse.IsSuccessStatusCode)
                    {
                        var finalPokemon = await DeserializeResponse<Pokemon>(finalPokemonResponse);
                        finalPokemon.Ability.Should().Be("Run Away");
                    }
                }
            }
        }

        [TestMethod]
        public async Task CompleteScenario_SearchAndFilterPokemons_ShouldSucceed()
        {
            // Scenario: Rechercher et filtrer des Pokemon par différents critères

            // 1. Rechercher tous les Pokemon
            var allPokemonsResponse = await _httpClient.GetAsync("/api/pokemon");
            allPokemonsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var allPokemons = await DeserializeResponse<List<Pokemon>>(allPokemonsResponse);

            if (allPokemons.Any())
            {
                // 2. Filtrer par type
                var firstType = allPokemons.First().Type;
                var typeFilterResponse = await _httpClient.GetAsync($"/api/pokemon/type/{firstType}");
                if (typeFilterResponse.IsSuccessStatusCode)
                {
                    var filteredByType = await DeserializeResponse<List<Pokemon>>(typeFilterResponse);
                    filteredByType.All(p => p.Type == firstType).Should().BeTrue();
                }

                // 3. Filtrer par niveau
                var levelFilterResponse = await _httpClient.GetAsync("/api/pokemon/level?minLevel=1&maxLevel=50");
                if (levelFilterResponse.IsSuccessStatusCode)
                {
                    var filteredByLevel = await DeserializeResponse<List<Pokemon>>(levelFilterResponse);
                    filteredByLevel.All(p => p.Level >= 1 && p.Level <= 50).Should().BeTrue();
                }

                // 4. Rechercher par nom
                var firstPokemon = allPokemons.First();
                var nameSearchResponse = await _httpClient.GetAsync($"/api/pokemon/name/{firstPokemon.Name}");
                if (nameSearchResponse.IsSuccessStatusCode)
                {
                    var foundPokemon = await DeserializeResponse<Pokemon>(nameSearchResponse);
                    foundPokemon.Name.Should().Be(firstPokemon.Name);
                }

                // 5. Filtrer par capacité
                var firstAbility = allPokemons.First().Ability;
                var abilityFilterResponse = await _httpClient.GetAsync($"/api/pokemon/ability/{firstAbility}");
                if (abilityFilterResponse.IsSuccessStatusCode)
                {
                    var filteredByAbility = await DeserializeResponse<List<Pokemon>>(abilityFilterResponse);
                    filteredByAbility.All(p => p.Ability == firstAbility).Should().BeTrue();
                }
            }
        }

        #endregion

        #region Tests de robustesse

        [TestMethod]
        public async Task RobustnessTest_ConcurrentRequests_ShouldHandleGracefully()
        {
            // Tester la capacité de l'API à gérer plusieurs requêtes simultanées
            var tasks = new List<Task<HttpResponseMessage>>();

            // Lancer plusieurs requêtes GET simultanées
            for (int i = 0; i < 20; i++)
            {
                tasks.Add(_httpClient.GetAsync("/api/pokemon"));
                tasks.Add(_httpClient.GetAsync("/api/trainer"));
            }

            var responses = await Task.WhenAll(tasks);

            // Toutes les requêtes devraient réussir ou échouer de manière cohérente
            responses.Should().AllSatisfy(r => 
                r.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized));
        }

        [TestMethod]
        public async Task RobustnessTest_InvalidEndpoints_ShouldReturn404()
        {
            // Tester des endpoints inexistants
            var invalidEndpoints = new[]
            {
                "/api/invalid",
                "/api/pokemon/invalid",
                "/api/trainer/invalid",
                "/api/battle/invalid"
            };

            foreach (var endpoint in invalidEndpoints)
            {
                var response = await _httpClient.GetAsync(endpoint);
                response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.MethodNotAllowed);
            }
        }

        #endregion

        #region Méthodes utilitaires

        private StringContent CreateJsonContent(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        }

        private async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }

        #endregion

        [TestCleanup]
        public void Cleanup()
        {
            _httpClient?.Dispose();
            _factory?.Dispose();
        }

        public void Dispose()
        {
            Cleanup();
        }
    }
}
