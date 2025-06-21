public class BattleRepository : IBattleRepository
{
    private readonly List<Battle> _battles;
    private int _nextId;

    public BattleRepository()
    {
        _battles = new List<Battle>();
        _nextId = 1;
        
        // Ajouter quelques combats d'exemple
        InitializeSampleBattles();
    }

    private void InitializeSampleBattles()
    {
        var battle1 = new Battle(1, "Ash Ketchum", 2, "Misty", "Arène de Cerulean City")
        {
            Id = _nextId++
        };
        battle1.SetWinner(1, "Ash Ketchum");
        battle1.Rounds.Add(new BattleRound(1, 25, "Pikachu", 54, "Psyduck"));
        battle1.Rounds[0].SetWinner(25, "Pikachu", "Attaque électrique super efficace");
        battle1.Notes = "Premier combat d'arène d'Ash";
        _battles.Add(battle1);

        var battle2 = new Battle(1, "Ash Ketchum", 4, "Gary Oak", "Route 22")
        {
            Id = _nextId++,
            BattleDate = DateTime.Now.AddDays(-7)
        };
        battle2.SetWinner(4, "Gary Oak");
        battle2.Rounds.Add(new BattleRound(1, 25, "Pikachu", 9, "Blastoise"));
        battle2.Rounds[0].SetWinner(9, "Blastoise", "Avantage de type eau contre électrique");
        battle2.Notes = "Rivalité légendaire";
        _battles.Add(battle2);

        var battle3 = new Battle(5, "May", 7, "Serena", "Contest Hall")
        {
            Id = _nextId++,
            BattleDate = DateTime.Now.AddDays(-3)
        };
        battle3.SetDraw();
        battle3.Notes = "Combat de coordination, égalité parfaite";
        _battles.Add(battle3);
    }

    public IReadOnlyCollection<Battle> GetAll()
    {
        return _battles.AsReadOnly();
    }

    public Battle GetById(int id)
    {
        return _battles.FirstOrDefault(b => b.Id == id);
    }

    public IReadOnlyCollection<Battle> GetBattlesByTrainer(int trainerId)
    {
        return _battles
            .Where(b => b.Trainer1Id == trainerId || b.Trainer2Id == trainerId)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyCollection<Battle> GetBattlesBetweenTrainers(int trainer1Id, int trainer2Id)
    {
        return _battles
            .Where(b => (b.Trainer1Id == trainer1Id && b.Trainer2Id == trainer2Id) ||
                       (b.Trainer1Id == trainer2Id && b.Trainer2Id == trainer1Id))
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyCollection<Battle> GetBattlesByDate(DateTime date)
    {
        return _battles
            .Where(b => b.BattleDate.Date == date.Date)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyCollection<Battle> GetBattlesByDateRange(DateTime startDate, DateTime endDate)
    {
        return _battles
            .Where(b => b.BattleDate.Date >= startDate.Date && b.BattleDate.Date <= endDate.Date)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyCollection<Battle> GetBattlesByResult(BattleResult result)
    {
        return _battles
            .Where(b => b.Result == result)
            .ToList()
            .AsReadOnly();
    }

    public void Add(Battle battle)
    {
        if (battle != null)
        {
            battle.Id = _nextId++;
            _battles.Add(battle);
        }
    }

    public void Update(Battle battle)
    {
        if (battle == null) return;

        var existingBattle = GetById(battle.Id);
        if (existingBattle != null)
        {
            var index = _battles.IndexOf(existingBattle);
            _battles[index] = battle;
        }
    }

    public void Delete(int id)
    {
        var battle = GetById(id);
        if (battle != null)
        {
            _battles.Remove(battle);
        }
    }
}
