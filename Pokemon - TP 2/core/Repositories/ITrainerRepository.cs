public interface ITrainerRepository
{
    IReadOnlyCollection<Trainer> GetAll();
    Trainer GetById(int id);
    Trainer GetByName(string name);
    void Add(Trainer trainer);
    void Update(Trainer trainer);
    void Delete(int id);
}
