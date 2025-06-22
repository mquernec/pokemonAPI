namespace PokemonAPI.Tests.Models
{
    /// <summary>
    /// DTOs pour les requêtes de test
    /// </summary>
    public class CreatePokemonRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Level { get; set; } = 1;
        public string Ability { get; set; } = string.Empty;
    }

    public class UpdatePokemonRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Level { get; set; } = 1;
        public string Ability { get; set; } = string.Empty;
    }

    public class ChangeAbilityRequest
    {
        public string NewAbility { get; set; } = string.Empty;
    }

    public class CreateTrainerRequest
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Region { get; set; } = string.Empty;
        public int BadgeCount { get; set; } = 0;
    }

    public class UpdateTrainerRequest
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Region { get; set; } = string.Empty;
        public int BadgeCount { get; set; }
    }

    public class AssignPokemonRequest
    {
        public int PokemonId { get; set; }
    }

    public class CreateBattleRequest
    {
        public int Trainer1Id { get; set; }
        public int Trainer2Id { get; set; }
        public string Location { get; set; } = string.Empty;
    }

    public class DeclareWinnerRequest
    {
        public int WinnerId { get; set; }
    }
}
