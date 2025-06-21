public class TrainerService : ITrainerService
{
    private readonly ITrainerRepository _trainerRepository;

    public TrainerService(ITrainerRepository trainerRepository)
    {
        _trainerRepository = trainerRepository;
    }

    public IReadOnlyCollection<Trainer> GetAllTrainers()
    {
        return _trainerRepository.GetAll();
    }

    public Trainer GetTrainerById(int id)
    {
        return _trainerRepository.GetById(id);
    }

    public Trainer GetTrainerByName(string name)
    {
        return _trainerRepository.GetByName(name);
    }

    public void CreateTrainer(Trainer trainer)
    {
        if (trainer == null)
            throw new ArgumentNullException(nameof(trainer));

        if (string.IsNullOrWhiteSpace(trainer.Name))
            throw new ArgumentException("Le nom du dresseur ne peut pas être vide.", nameof(trainer));

        _trainerRepository.Add(trainer);
    }

    public void UpdateTrainer(Trainer trainer)
    {
        if (trainer == null)
            throw new ArgumentNullException(nameof(trainer));

        var existingTrainer = _trainerRepository.GetById(trainer.Id);
        if (existingTrainer == null)
            throw new InvalidOperationException($"Aucun dresseur trouvé avec l'ID {trainer.Id}");

        _trainerRepository.Update(trainer);
    }

    public void DeleteTrainer(int id)
    {
        var trainer = _trainerRepository.GetById(id);
        if (trainer == null)
            throw new InvalidOperationException($"Aucun dresseur trouvé avec l'ID {id}");

        _trainerRepository.Delete(id);
    }

    public void AssignPokemonToTrainer(int trainerId, Pokemon pokemon)
    {
        if (pokemon == null)
            throw new ArgumentNullException(nameof(pokemon));

        var trainer = _trainerRepository.GetById(trainerId);
        if (trainer == null)
            throw new InvalidOperationException($"Aucun dresseur trouvé avec l'ID {trainerId}");

        if (trainer.PokemonTeam.Count >= 6)
            throw new InvalidOperationException("Un dresseur ne peut avoir plus de 6 Pokémon dans son équipe.");

        trainer.AddPokemon(pokemon);
        _trainerRepository.Update(trainer);
    }

    public void RemovePokemonFromTrainer(int trainerId, Pokemon pokemon)
    {
        if (pokemon == null)
            throw new ArgumentNullException(nameof(pokemon));

        var trainer = _trainerRepository.GetById(trainerId);
        if (trainer == null)
            throw new InvalidOperationException($"Aucun dresseur trouvé avec l'ID {trainerId}");

        trainer.RemovePokemon(pokemon);
        _trainerRepository.Update(trainer);
    }

    public IReadOnlyCollection<Trainer> GetTrainersByRegion(string region)
    {
        if (string.IsNullOrWhiteSpace(region))
            return new List<Trainer>().AsReadOnly();

        return _trainerRepository.GetAll()
            .Where(t => t.Region.Equals(region, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyCollection<Battle> GetTrainerBattleHistory(int trainerId)
    {
        // Cette méthode nécessiterait une injection de IBattleService
        // Pour l'instant, on retourne une liste vide
        // En pratique, il faudrait injecter IBattleService dans le constructeur
        return new List<Battle>().AsReadOnly();
    }

    public BattleStatistics GetTrainerBattleStatistics(int trainerId)
    {
        // Cette méthode nécessiterait une injection de IBattleService
        // Pour l'instant, on retourne des statistiques par défaut
        var trainer = _trainerRepository.GetById(trainerId);
        if (trainer == null)
            throw new InvalidOperationException($"Aucun dresseur trouvé avec l'ID {trainerId}");

        return new BattleStatistics
        {
            TrainerId = trainerId,
            TrainerName = trainer.Name,
            TotalBattles = 0,
            Wins = 0,
            Losses = 0,
            Draws = 0,
            WinRate = 0,
            LastBattleDate = null,
            FavoriteOpponent = "Aucun"
        };
    }
}
