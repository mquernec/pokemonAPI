public interface IPokemonService
{
    IReadOnlyCollection<Pokemon> GetAllPokemon();
    Pokemon GetPokemonById(int id);
    Pokemon GetPokemonByName(string name);
    void CreatePokemon(Pokemon pokemon);
    void UpdatePokemon(Pokemon pokemon);
    void DeletePokemon(int id);
    IReadOnlyCollection<Pokemon> GetPokemonByType(string type);
    IReadOnlyCollection<Pokemon> GetPokemonByLevel(int minLevel, int maxLevel);
    IReadOnlyCollection<Pokemon> GetPokemonByAbility(string ability);
    void LevelUpPokemon(int id);
    void ChangePokemonAbility(int id, string newAbility);
}