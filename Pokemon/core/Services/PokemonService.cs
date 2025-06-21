public class PokemonService : IPokemonService
{
    private readonly IPokemonRepository _pokemonRepository;

    public PokemonService(IPokemonRepository pokemonRepository)
    {
        _pokemonRepository = pokemonRepository;
    }

    public IReadOnlyCollection<Pokemon> GetAllPokemon()
    {
        return _pokemonRepository.Get();
    }

    public Pokemon GetPokemonById(int id)
    {
        return _pokemonRepository.GetById(id);
    }

    public Pokemon GetPokemonByName(string name)
    {
        return _pokemonRepository.GetByName(name);
    }

    public void CreatePokemon(Pokemon pokemon)
    {
        if (pokemon == null)
            throw new ArgumentNullException(nameof(pokemon));

        if (string.IsNullOrWhiteSpace(pokemon.Name))
            throw new ArgumentException("Le nom du Pokémon ne peut pas être vide.", nameof(pokemon));

        if (pokemon.Level <= 0)
            throw new ArgumentException("Le niveau du Pokémon doit être supérieur à 0.", nameof(pokemon));

        _pokemonRepository.Add(pokemon);
    }

    public void UpdatePokemon(Pokemon pokemon)
    {
        if (pokemon == null)
            throw new ArgumentNullException(nameof(pokemon));

        var existingPokemon = _pokemonRepository.GetById(pokemon.Id);
        if (existingPokemon == null)
            throw new InvalidOperationException($"Aucun Pokémon trouvé avec l'ID {pokemon.Id}");

        if (string.IsNullOrWhiteSpace(pokemon.Name))
            throw new ArgumentException("Le nom du Pokémon ne peut pas être vide.", nameof(pokemon));

        if (pokemon.Level <= 0)
            throw new ArgumentException("Le niveau du Pokémon doit être supérieur à 0.", nameof(pokemon));

        _pokemonRepository.Update(pokemon);
    }

    public void DeletePokemon(int id)
    {
        var pokemon = _pokemonRepository.GetById(id);
        if (pokemon == null)
            throw new InvalidOperationException($"Aucun Pokémon trouvé avec l'ID {id}");

        _pokemonRepository.Delete(id);
    }

    public IReadOnlyCollection<Pokemon> GetPokemonByType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return new List<Pokemon>().AsReadOnly();

        return _pokemonRepository.Get()
            .Where(p => p.Type.Equals(type, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyCollection<Pokemon> GetPokemonByLevel(int minLevel, int maxLevel)
    {
        if (minLevel > maxLevel)
            throw new ArgumentException("Le niveau minimum ne peut pas être supérieur au niveau maximum.");

        return _pokemonRepository.Get()
            .Where(p => p.Level >= minLevel && p.Level <= maxLevel)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyCollection<Pokemon> GetPokemonByAbility(string ability)
    {
        if (string.IsNullOrWhiteSpace(ability))
            return new List<Pokemon>().AsReadOnly();

        return _pokemonRepository.Get()
            .Where(p => p.Ability.Equals(ability, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }

    public void LevelUpPokemon(int id)
    {
        var pokemon = _pokemonRepository.GetById(id);
        if (pokemon == null)
            throw new InvalidOperationException($"Aucun Pokémon trouvé avec l'ID {id}");

        if (pokemon.Level >= 100)
            throw new InvalidOperationException("Le Pokémon a déjà atteint le niveau maximum (100).");

        pokemon.Level++;
        _pokemonRepository.Update(pokemon);
    }

    public void ChangePokemonAbility(int id, string newAbility)
    {
        if (string.IsNullOrWhiteSpace(newAbility))
            throw new ArgumentException("La nouvelle capacité ne peut pas être vide.", nameof(newAbility));

        var pokemon = _pokemonRepository.GetById(id);
        if (pokemon == null)
            throw new InvalidOperationException($"Aucun Pokémon trouvé avec l'ID {id}");

        pokemon.Ability = newAbility;
        _pokemonRepository.Update(pokemon);
    }
}
