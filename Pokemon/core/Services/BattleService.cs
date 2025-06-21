public class BattleService : IBattleService
{
    private readonly IBattleRepository _battleRepository;
    private readonly ITrainerRepository _trainerRepository;
    private readonly IPokemonRepository _pokemonRepository;

    public BattleService(IBattleRepository battleRepository, ITrainerRepository trainerRepository, IPokemonRepository pokemonRepository)
    {
        _battleRepository = battleRepository;
        _trainerRepository = trainerRepository;
        _pokemonRepository = pokemonRepository;
    }

    public IReadOnlyCollection<Battle> GetAllBattles()
    {
        return _battleRepository.GetAll();
    }

    public Battle GetBattleById(int id)
    {
        return _battleRepository.GetById(id);
    }

    public IReadOnlyCollection<Battle> GetBattlesByTrainer(int trainerId)
    {
        return _battleRepository.GetBattlesByTrainer(trainerId);
    }

    public IReadOnlyCollection<Battle> GetBattleHistory(int trainer1Id, int trainer2Id)
    {
        return _battleRepository.GetBattlesBetweenTrainers(trainer1Id, trainer2Id);
    }

    public Battle CreateBattle(int trainer1Id, int trainer2Id, string location = "")
    {
        if (trainer1Id == trainer2Id)
            throw new ArgumentException("Un dresseur ne peut pas se battre contre lui-même.");

        var trainer1 = _trainerRepository.GetById(trainer1Id);
        var trainer2 = _trainerRepository.GetById(trainer2Id);

        if (trainer1 == null)
            throw new InvalidOperationException($"Dresseur avec l'ID {trainer1Id} introuvable.");
        if (trainer2 == null)
            throw new InvalidOperationException($"Dresseur avec l'ID {trainer2Id} introuvable.");

        var battle = new Battle(trainer1Id, trainer1.Name, trainer2Id, trainer2.Name, location);
        _battleRepository.Add(battle);

        return battle;
    }

    public void StartBattle(int battleId)
    {
        var battle = _battleRepository.GetById(battleId);
        if (battle == null)
            throw new InvalidOperationException($"Combat avec l'ID {battleId} introuvable.");

        if (battle.Result != BattleResult.InProgress)
            throw new InvalidOperationException("Ce combat a déjà été terminé ou annulé.");

        // Le combat est déjà en cours par défaut
        _battleRepository.Update(battle);
    }

    public void AddRound(int battleId, int pokemon1Id, int pokemon2Id, int? winnerPokemonId = null, string description = "")
    {
        var battle = _battleRepository.GetById(battleId);
        if (battle == null)
            throw new InvalidOperationException($"Combat avec l'ID {battleId} introuvable.");

        if (battle.Result != BattleResult.InProgress)
            throw new InvalidOperationException("Impossible d'ajouter un round à un combat terminé.");

        var pokemon1 = _pokemonRepository.GetById(pokemon1Id);
        var pokemon2 = _pokemonRepository.GetById(pokemon2Id);

        if (pokemon1 == null || pokemon2 == null)
            throw new InvalidOperationException("Un ou plusieurs Pokémon sont introuvables.");

        var roundNumber = battle.Rounds.Count + 1;
        var round = new BattleRound(roundNumber, pokemon1Id, pokemon1.Name, pokemon2Id, pokemon2.Name);

        if (winnerPokemonId.HasValue)
        {
            var winnerPokemon = winnerPokemonId == pokemon1Id ? pokemon1 : pokemon2;
            round.SetWinner(winnerPokemonId.Value, winnerPokemon.Name, description);
        }

        battle.Rounds.Add(round);
        _battleRepository.Update(battle);
    }

    public void SetBattleWinner(int battleId, int winnerId)
    {
        var battle = _battleRepository.GetById(battleId);
        if (battle == null)
            throw new InvalidOperationException($"Combat avec l'ID {battleId} introuvable.");

        if (battle.Result != BattleResult.InProgress)
            throw new InvalidOperationException("Ce combat a déjà été terminé.");

        if (winnerId != battle.Trainer1Id && winnerId != battle.Trainer2Id)
            throw new ArgumentException("L'ID du gagnant doit correspondre à l'un des participants.");

        var winnerName = winnerId == battle.Trainer1Id ? battle.Trainer1Name : battle.Trainer2Name;
        battle.SetWinner(winnerId, winnerName);

        _battleRepository.Update(battle);
    }

    public void SetBattleDraw(int battleId)
    {
        var battle = _battleRepository.GetById(battleId);
        if (battle == null)
            throw new InvalidOperationException($"Combat avec l'ID {battleId} introuvable.");

        if (battle.Result != BattleResult.InProgress)
            throw new InvalidOperationException("Ce combat a déjà été terminé.");

        battle.SetDraw();
        _battleRepository.Update(battle);
    }

    public void CancelBattle(int battleId)
    {
        var battle = _battleRepository.GetById(battleId);
        if (battle == null)
            throw new InvalidOperationException($"Combat avec l'ID {battleId} introuvable.");

        battle.Result = BattleResult.Cancelled;
        _battleRepository.Update(battle);
    }

    public void AddBattleNotes(int battleId, string notes)
    {
        var battle = _battleRepository.GetById(battleId);
        if (battle == null)
            throw new InvalidOperationException($"Combat avec l'ID {battleId} introuvable.");

        battle.Notes = notes;
        _battleRepository.Update(battle);
    }

    public BattleStatistics GetTrainerStatistics(int trainerId)
    {
        var trainer = _trainerRepository.GetById(trainerId);
        if (trainer == null)
            throw new InvalidOperationException($"Dresseur avec l'ID {trainerId} introuvable.");

        var battles = _battleRepository.GetBattlesByTrainer(trainerId)
            .Where(b => b.Result == BattleResult.Completed || b.Result == BattleResult.Draw)
            .ToList();

        var wins = battles.Count(b => b.WinnerId == trainerId);
        var losses = battles.Count(b => b.WinnerId.HasValue && b.WinnerId != trainerId);
        var draws = battles.Count(b => b.Result == BattleResult.Draw);

        var winRate = battles.Count > 0 ? (double)wins / battles.Count : 0;

        var opponentCounts = battles
            .Select(b => b.Trainer1Id == trainerId ? b.Trainer2Name : b.Trainer1Name)
            .GroupBy(name => name)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        return new BattleStatistics
        {
            TrainerId = trainerId,
            TrainerName = trainer.Name,
            TotalBattles = battles.Count,
            Wins = wins,
            Losses = losses,
            Draws = draws,
            WinRate = winRate,
            LastBattleDate = battles.OrderByDescending(b => b.BattleDate).FirstOrDefault()?.BattleDate,
            FavoriteOpponent = opponentCounts?.Key ?? "Aucun"
        };
    }

    public IReadOnlyCollection<Battle> GetRecentBattles(int days = 30)
    {
        var startDate = DateTime.Now.AddDays(-days);
        return _battleRepository.GetBattlesByDateRange(startDate, DateTime.Now);
    }

    public void DeleteBattle(int id)
    {
        var battle = _battleRepository.GetById(id);
        if (battle == null)
            throw new InvalidOperationException($"Combat avec l'ID {id} introuvable.");

        _battleRepository.Delete(id);
    }
}
