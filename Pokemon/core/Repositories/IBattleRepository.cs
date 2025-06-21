public interface IBattleRepository
{
    IReadOnlyCollection<Battle> GetAll();
    Battle GetById(int id);
    IReadOnlyCollection<Battle> GetBattlesByTrainer(int trainerId);
    IReadOnlyCollection<Battle> GetBattlesBetweenTrainers(int trainer1Id, int trainer2Id);
    IReadOnlyCollection<Battle> GetBattlesByDate(DateTime date);
    IReadOnlyCollection<Battle> GetBattlesByDateRange(DateTime startDate, DateTime endDate);
    IReadOnlyCollection<Battle> GetBattlesByResult(BattleResult result);
    void Add(Battle battle);
    void Update(Battle battle);
    void Delete(int id);
}
