public interface IPokemonRepository
{
    IReadOnlyCollection<Pokemon> Get();
    Pokemon GetById(int id);
    Pokemon GetByName(string name);
    void Add(Pokemon pokemon);
    void Update(Pokemon pokemon);
    void Delete(int id);
}
