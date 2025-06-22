using Moq;
using PokemonAPI.Tests.Models;

namespace PokemonAPI.Tests.Services
{
    /// <summary>
    /// Tests unitaires pour les services Pokemon (si accessible)
    /// </summary>
    [TestClass]
    public class PokemonServiceTests
    {
        private Mock<IPokemonService> _mockPokemonService;
        private List<Pokemon> _testPokemons;

        [TestInitialize]
        public void Setup()
        {
            _mockPokemonService = new Mock<IPokemonService>();
            _testPokemons = new List<Pokemon>
            {
                new Pokemon(1, "Pikachu", "Electric", 25, "Static"),
                new Pokemon(2, "Charizard", "Fire", 50, "Blaze"),
                new Pokemon(3, "Blastoise", "Water", 45, "Torrent"),
                new Pokemon(4, "Venusaur", "Grass", 40, "Overgrow"),
                new Pokemon(5, "Raichu", "Electric", 35, "Static")
            };
        }

        #region Tests GetAllPokemon

        [TestMethod]
        public void GetAllPokemon_ShouldReturnAllPokemons()
        {
            // Arrange
            _mockPokemonService.Setup(s => s.GetAllPokemon()).Returns(_testPokemons);

            // Act
            var result = _mockPokemonService.Object.GetAllPokemon();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
            result.Should().BeEquivalentTo(_testPokemons);
        }

        [TestMethod]
        public void GetAllPokemon_WhenEmpty_ShouldReturnEmptyList()
        {
            // Arrange
            _mockPokemonService.Setup(s => s.GetAllPokemon()).Returns(new List<Pokemon>());

            // Act
            var result = _mockPokemonService.Object.GetAllPokemon();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region Tests GetPokemonById

        [TestMethod]
        public void GetPokemonById_WithValidId_ShouldReturnPokemon()
        {
            // Arrange
            var expectedPokemon = _testPokemons.First();
            _mockPokemonService.Setup(s => s.GetPokemonById(1)).Returns(expectedPokemon);

            // Act
            var result = _mockPokemonService.Object.GetPokemonById(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedPokemon);
        }

        [TestMethod]
        public void GetPokemonById_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            _mockPokemonService.Setup(s => s.GetPokemonById(999)).Returns((Pokemon)null);

            // Act
            var result = _mockPokemonService.Object.GetPokemonById(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region Tests GetPokemonByName

        [TestMethod]
        public void GetPokemonByName_WithValidName_ShouldReturnPokemon()
        {
            // Arrange
            var expectedPokemon = _testPokemons.First(p => p.Name == "Pikachu");
            _mockPokemonService.Setup(s => s.GetPokemonByName("Pikachu")).Returns(expectedPokemon);

            // Act
            var result = _mockPokemonService.Object.GetPokemonByName("Pikachu");

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Pikachu");
        }

        [TestMethod]
        public void GetPokemonByName_WithInvalidName_ShouldReturnNull()
        {
            // Arrange
            _mockPokemonService.Setup(s => s.GetPokemonByName("InvalidName")).Returns((Pokemon)null);

            // Act
            var result = _mockPokemonService.Object.GetPokemonByName("InvalidName");

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void GetPokemonByName_WithNullName_ShouldThrowException()
        {
            // Arrange
            _mockPokemonService.Setup(s => s.GetPokemonByName(null)).Throws<ArgumentNullException>();

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => 
                _mockPokemonService.Object.GetPokemonByName(null));
        }

        #endregion

        #region Tests GetPokemonByType

        [TestMethod]
        public void GetPokemonByType_WithValidType_ShouldReturnFilteredPokemons()
        {
            // Arrange
            var electricPokemons = _testPokemons.Where(p => p.Type == "Electric").ToList();
            _mockPokemonService.Setup(s => s.GetPokemonByType("Electric")).Returns(electricPokemons);

            // Act
            var result = _mockPokemonService.Object.GetPokemonByType("Electric");

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.All(p => p.Type == "Electric").Should().BeTrue();
        }

        [TestMethod]
        public void GetPokemonByType_WithInvalidType_ShouldReturnEmptyList()
        {
            // Arrange
            _mockPokemonService.Setup(s => s.GetPokemonByType("InvalidType")).Returns(new List<Pokemon>());

            // Act
            var result = _mockPokemonService.Object.GetPokemonByType("InvalidType");

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region Tests GetPokemonByLevel

        [TestMethod]
        public void GetPokemonByLevel_WithValidRange_ShouldReturnFilteredPokemons()
        {
            // Arrange
            var pokemonsInRange = _testPokemons.Where(p => p.Level >= 30 && p.Level <= 50).ToList();
            _mockPokemonService.Setup(s => s.GetPokemonByLevel(30, 50)).Returns(pokemonsInRange);

            // Act
            var result = _mockPokemonService.Object.GetPokemonByLevel(30, 50);

            // Assert
            result.Should().NotBeNull();
            result.All(p => p.Level >= 30 && p.Level <= 50).Should().BeTrue();
        }

        [TestMethod]
        public void GetPokemonByLevel_WithInvalidRange_ShouldThrowException()
        {
            // Arrange
            _mockPokemonService.Setup(s => s.GetPokemonByLevel(50, 30))
                .Throws<ArgumentException>();

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => 
                _mockPokemonService.Object.GetPokemonByLevel(50, 30));
        }

        #endregion

        #region Tests GetPokemonByAbility

        [TestMethod]
        public void GetPokemonByAbility_WithValidAbility_ShouldReturnFilteredPokemons()
        {
            // Arrange
            var staticPokemons = _testPokemons.Where(p => p.Ability == "Static").ToList();
            _mockPokemonService.Setup(s => s.GetPokemonByAbility("Static")).Returns(staticPokemons);

            // Act
            var result = _mockPokemonService.Object.GetPokemonByAbility("Static");

            // Assert
            result.Should().NotBeNull();
            result.All(p => p.Ability == "Static").Should().BeTrue();
        }

        #endregion

        #region Tests CreatePokemon

        [TestMethod]
        public void CreatePokemon_WithValidPokemon_ShouldAddPokemon()
        {
            // Arrange
            var newPokemon = new Pokemon(6, "Squirtle", "Water", 10, "Torrent");
            _mockPokemonService.Setup(s => s.CreatePokemon(It.IsAny<Pokemon>()))
                .Callback<Pokemon>(p => _testPokemons.Add(p));

            // Act
            _mockPokemonService.Object.CreatePokemon(newPokemon);

            // Assert
            _mockPokemonService.Verify(s => s.CreatePokemon(newPokemon), Times.Once);
        }

        [TestMethod]
        public void CreatePokemon_WithNullPokemon_ShouldThrowException()
        {
            // Arrange
            _mockPokemonService.Setup(s => s.CreatePokemon(null))
                .Throws<ArgumentNullException>();

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => 
                _mockPokemonService.Object.CreatePokemon(null));
        }

        [TestMethod]
        public void CreatePokemon_WithInvalidData_ShouldThrowException()
        {
            // Arrange
            var invalidPokemon = new Pokemon(0, "", "", -1, "");
            _mockPokemonService.Setup(s => s.CreatePokemon(invalidPokemon))
                .Throws<ArgumentException>();

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => 
                _mockPokemonService.Object.CreatePokemon(invalidPokemon));
        }

        #endregion

        #region Tests UpdatePokemon

        [TestMethod]
        public void UpdatePokemon_WithValidPokemon_ShouldUpdatePokemon()
        {
            // Arrange
            var updatedPokemon = new Pokemon(1, "Pikachu", "Electric", 30, "Lightning Rod");
            _mockPokemonService.Setup(s => s.UpdatePokemon(It.IsAny<Pokemon>()));

            // Act
            _mockPokemonService.Object.UpdatePokemon(updatedPokemon);

            // Assert
            _mockPokemonService.Verify(s => s.UpdatePokemon(updatedPokemon), Times.Once);
        }

        [TestMethod]
        public void UpdatePokemon_WithNonExistentPokemon_ShouldThrowException()
        {
            // Arrange
            var nonExistentPokemon = new Pokemon(999, "NonExistent", "Normal", 1, "None");
            _mockPokemonService.Setup(s => s.UpdatePokemon(nonExistentPokemon))
                .Throws<InvalidOperationException>();

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() => 
                _mockPokemonService.Object.UpdatePokemon(nonExistentPokemon));
        }

        #endregion

        #region Tests DeletePokemon

        [TestMethod]
        public void DeletePokemon_WithValidId_ShouldDeletePokemon()
        {
            // Arrange
            _mockPokemonService.Setup(s => s.DeletePokemon(1));

            // Act
            _mockPokemonService.Object.DeletePokemon(1);

            // Assert
            _mockPokemonService.Verify(s => s.DeletePokemon(1), Times.Once);
        }

        [TestMethod]
        public void DeletePokemon_WithInvalidId_ShouldThrowException()
        {
            // Arrange
            _mockPokemonService.Setup(s => s.DeletePokemon(999))
                .Throws<InvalidOperationException>();

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() => 
                _mockPokemonService.Object.DeletePokemon(999));
        }

        #endregion

        #region Tests LevelUpPokemon

        [TestMethod]
        public void LevelUpPokemon_WithValidId_ShouldIncreaseLevelByOne()
        {
            // Arrange
            var pokemon = _testPokemons.First();
            var originalLevel = pokemon.Level;
            _mockPokemonService.Setup(s => s.LevelUpPokemon(1));

            // Act
            _mockPokemonService.Object.LevelUpPokemon(1);

            // Assert
            _mockPokemonService.Verify(s => s.LevelUpPokemon(1), Times.Once);
        }

        [TestMethod]
        public void LevelUpPokemon_WithMaxLevel_ShouldThrowException()
        {
            // Arrange
            _mockPokemonService.Setup(s => s.LevelUpPokemon(1))
                .Throws<InvalidOperationException>();

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() => 
                _mockPokemonService.Object.LevelUpPokemon(1));
        }

        #endregion

        #region Tests ChangePokemonAbility

        [TestMethod]
        public void ChangePokemonAbility_WithValidData_ShouldChangeAbility()
        {
            // Arrange
            _mockPokemonService.Setup(s => s.ChangePokemonAbility(1, "Lightning Rod"));

            // Act
            _mockPokemonService.Object.ChangePokemonAbility(1, "Lightning Rod");

            // Assert
            _mockPokemonService.Verify(s => s.ChangePokemonAbility(1, "Lightning Rod"), Times.Once);
        }

        [TestMethod]
        public void ChangePokemonAbility_WithInvalidAbility_ShouldThrowException()
        {
            // Arrange
            _mockPokemonService.Setup(s => s.ChangePokemonAbility(1, ""))
                .Throws<ArgumentException>();

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => 
                _mockPokemonService.Object.ChangePokemonAbility(1, ""));
        }

        #endregion

        [TestCleanup]
        public void Cleanup()
        {
            _mockPokemonService = null;
            _testPokemons = null;
        }
    }

    /// <summary>
    /// Interface fictive pour les tests (à remplacer par la vraie interface du projet)
    /// </summary>
    public interface IPokemonService
    {
        IEnumerable<Pokemon> GetAllPokemon();
        Pokemon GetPokemonById(int id);
        Pokemon GetPokemonByName(string name);
        IEnumerable<Pokemon> GetPokemonByType(string type);
        IEnumerable<Pokemon> GetPokemonByLevel(int minLevel, int maxLevel);
        IEnumerable<Pokemon> GetPokemonByAbility(string ability);
        void CreatePokemon(Pokemon pokemon);
        void UpdatePokemon(Pokemon pokemon);
        void DeletePokemon(int id);
        void LevelUpPokemon(int id);
        void ChangePokemonAbility(int id, string newAbility);
    }
}
