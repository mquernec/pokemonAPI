public interface IPokemonService
{
    IReadOnlyCollection<string> GetAllPokemon();
    string GetPokemonByName(string name);   
}