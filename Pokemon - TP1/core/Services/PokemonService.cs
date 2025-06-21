public class PokemonService : IPokemonService
{
    private readonly IPokemonRepository _pokemonRepository;

    public PokemonService(IPokemonRepository pokemonRepository)
    {
        _pokemonRepository = pokemonRepository;
    }

    public IReadOnlyCollection<string> GetAllPokemon()
    {
        return _pokemonRepository.Get();
    }

    public string GetPokemonByName(string name)
    {
        return _pokemonRepository.Get().FirstOrDefault(p => p.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}