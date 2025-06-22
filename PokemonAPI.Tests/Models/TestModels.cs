namespace PokemonAPI.Tests.Models
{
    /// <summary>
    /// Modèle Pokemon pour les tests
    /// </summary>
    public class Pokemon
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Level { get; set; }
        public string Ability { get; set; } = string.Empty;

        public Pokemon() { }

        public Pokemon(int id, string name, string type, int level, string ability)
        {
            Id = id;
            Name = name;
            Type = type;
            Level = level;
            Ability = ability;
        }
    }

    /// <summary>
    /// Modèle Trainer pour les tests
    /// </summary>
    public class Trainer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Region { get; set; } = string.Empty;
        public List<Pokemon> PokemonTeam { get; set; } = new();
        public int BadgeCount { get; set; }
        public DateTime StartDate { get; set; }
    }

    /// <summary>
    /// Modèle Battle pour les tests
    /// </summary>
    public class Battle
    {
        public int Id { get; set; }
        public int Trainer1Id { get; set; }
        public int Trainer2Id { get; set; }
        public int? WinnerId { get; set; }
        public DateTime BattleDate { get; set; }
        public string Location { get; set; } = string.Empty;
    }
}
