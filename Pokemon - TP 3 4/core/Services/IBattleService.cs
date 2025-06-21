public interface IBattleService
{
    IReadOnlyCollection<Battle> GetAllBattles();
    Battle GetBattleById(int id);
    IReadOnlyCollection<Battle> GetBattlesByTrainer(int trainerId);
    IReadOnlyCollection<Battle> GetBattleHistory(int trainer1Id, int trainer2Id);
    Battle CreateBattle(int trainer1Id, int trainer2Id, string location = "");
    void StartBattle(int battleId);
    void AddRound(int battleId, int pokemon1Id, int pokemon2Id, int? winnerPokemonId = null, string description = "");
    void SetBattleWinner(int battleId, int winnerId);
    void SetBattleDraw(int battleId);
    void CancelBattle(int battleId);
    void AddBattleNotes(int battleId, string notes);
    BattleStatistics GetTrainerStatistics(int trainerId);
    IReadOnlyCollection<Battle> GetRecentBattles(int days = 30);
    void DeleteBattle(int id);
}

public class BattleStatistics
{
    public int TrainerId { get; set; }
    public string TrainerName { get; set; }
    public int TotalBattles { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public double WinRate { get; set; }
    public DateTime? LastBattleDate { get; set; }
    public string FavoriteOpponent { get; set; }

    public override string ToString()
    {
        return $"{TrainerName}: {Wins}V-{Losses}D-{Draws}N (Taux: {WinRate:P1})";
    }
}
