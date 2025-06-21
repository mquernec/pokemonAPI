public class TrainerRepository : ITrainerRepository
{
    private readonly List<Trainer> _trainers;

    public TrainerRepository()
    {
        _trainers = new List<Trainer>
        {
            new Trainer(1, "Ash Ketchum", 16, "Kanto", 8),
            new Trainer(2, "Misty", 12, "Kanto", 4),
            new Trainer(3, "Brock", 15, "Kanto", 2),
            new Trainer(4, "Gary Oak", 16, "Kanto", 10),
            new Trainer(5, "May", 14, "Hoenn", 3),
            new Trainer(6, "Dawn", 13, "Sinnoh", 5),
            new Trainer(7, "Serena", 14, "Kalos", 2),
            new Trainer(8, "Chloe", 12, "Galar", 1)
        };
    }

    public IReadOnlyCollection<Trainer> GetAll()
    {
        return _trainers.AsReadOnly();
    }

    public Trainer GetById(int id)
    {
        return _trainers.FirstOrDefault(t => t.Id == id);
    }

    public Trainer GetByName(string name)
    {
        return _trainers.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public void Add(Trainer trainer)
    {
        if (trainer != null && !_trainers.Any(t => t.Id == trainer.Id))
        {
            _trainers.Add(trainer);
        }
    }

    public void Update(Trainer trainer)
    {
        var existingTrainer = GetById(trainer.Id);
        if (existingTrainer != null)
        {
            var index = _trainers.IndexOf(existingTrainer);
            _trainers[index] = trainer;
        }
    }

    public void Delete(int id)
    {
        var trainer = GetById(id);
        if (trainer != null)
        {
            _trainers.Remove(trainer);
        }
    }
}
