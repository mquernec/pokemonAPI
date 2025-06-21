public class Trainer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public string Region { get; set; }
    public List<Pokemon> PokemonTeam { get; set; }
    public int BadgeCount { get; set; }
    public DateTime StartDate { get; set; }

    public Trainer(int id, string name, int age, string region, int badgeCount = 0)
    {
        Id = id;
        Name = name;
        Age = age;
        Region = region;
        PokemonTeam = new List<Pokemon>();
        BadgeCount = badgeCount;
        StartDate = DateTime.Now;
    }

    public void AddPokemon(Pokemon pokemon)
    {
        if (PokemonTeam.Count < 6) // Limite de 6 Pokémon dans une équipe
        {
            PokemonTeam.Add(pokemon);
        }
    }

    public void RemovePokemon(Pokemon pokemon)
    {
        PokemonTeam.Remove(pokemon);
    }

    public override string ToString()
    {
        return $"{Name} (ID: {Id}, Age: {Age}, Region: {Region}, Badges: {BadgeCount}, Pokémon: {PokemonTeam.Count})";
    }
}
