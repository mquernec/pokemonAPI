public interface ITrainerService
{
    IReadOnlyCollection<Trainer> GetAllTrainers();
    Trainer GetTrainerById(int id);
    Trainer GetTrainerByName(string name);
    void CreateTrainer(Trainer trainer);
    void UpdateTrainer(Trainer trainer);
    void DeleteTrainer(int id);
    void AssignPokemonToTrainer(int trainerId, Pokemon pokemon);
    void RemovePokemonFromTrainer(int trainerId, Pokemon pokemon);
    IReadOnlyCollection<Trainer> GetTrainersByRegion(string region);
    IReadOnlyCollection<Battle> GetTrainerBattleHistory(int trainerId);
    BattleStatistics GetTrainerBattleStatistics(int trainerId);
}
