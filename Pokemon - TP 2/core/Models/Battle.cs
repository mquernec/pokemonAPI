public class Battle
{
    public int Id { get; set; }
    public int Trainer1Id { get; set; }
    public int Trainer2Id { get; set; }
    public string Trainer1Name { get; set; }
    public string Trainer2Name { get; set; }
    public int? WinnerId { get; set; }
    public string WinnerName { get; set; }
    public DateTime BattleDate { get; set; }
    public string Location { get; set; }
    public BattleResult Result { get; set; }
    public List<BattleRound> Rounds { get; set; }
    public string Notes { get; set; }

    public Battle()
    {
        Rounds = new List<BattleRound>();
        BattleDate = DateTime.Now;
    }

    public Battle(int trainer1Id, string trainer1Name, int trainer2Id, string trainer2Name, string location = "")
    {
        Trainer1Id = trainer1Id;
        Trainer1Name = trainer1Name;
        Trainer2Id = trainer2Id;
        Trainer2Name = trainer2Name;
        Location = location;
        BattleDate = DateTime.Now;
        Rounds = new List<BattleRound>();
        Result = BattleResult.InProgress;
    }

    public void SetWinner(int winnerId, string winnerName)
    {
        WinnerId = winnerId;
        WinnerName = winnerName;
        Result = BattleResult.Completed;
    }

    public void SetDraw()
    {
        WinnerId = null;
        WinnerName = "Match nul";
        Result = BattleResult.Draw;
    }

    public override string ToString()
    {
        var status = Result switch
        {
            BattleResult.Completed => $"Gagné par {WinnerName}",
            BattleResult.Draw => "Match nul",
            BattleResult.InProgress => "En cours",
            BattleResult.Cancelled => "Annulé",
            _ => "Statut inconnu"
        };

        return $"Combat {Id}: {Trainer1Name} vs {Trainer2Name} - {status} ({BattleDate:dd/MM/yyyy})";
    }
}

public enum BattleResult
{
    InProgress,
    Completed,
    Draw,
    Cancelled
}

public class BattleRound
{
    public int RoundNumber { get; set; }
    public int Pokemon1Id { get; set; }
    public string Pokemon1Name { get; set; }
    public int Pokemon2Id { get; set; }
    public string Pokemon2Name { get; set; }
    public int? WinnerPokemonId { get; set; }
    public string WinnerPokemonName { get; set; }
    public string Description { get; set; }

    public BattleRound(int roundNumber, int pokemon1Id, string pokemon1Name, int pokemon2Id, string pokemon2Name)
    {
        RoundNumber = roundNumber;
        Pokemon1Id = pokemon1Id;
        Pokemon1Name = pokemon1Name;
        Pokemon2Id = pokemon2Id;
        Pokemon2Name = pokemon2Name;
    }

    public void SetWinner(int winnerPokemonId, string winnerPokemonName, string description = "")
    {
        WinnerPokemonId = winnerPokemonId;
        WinnerPokemonName = winnerPokemonName;
        Description = description;
    }
}
