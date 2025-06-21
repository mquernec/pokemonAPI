
public class Pokemon
{
    public int Id { get; set; }


    public string Name { get; set; }
    public string Type { get; set; }
    public int Level { get; set; }
    public string Ability { get; set; }

    public Pokemon(int id, string name, string type, int level, string ability)
    {
        Id = id;
        Name = name;
        Type = type;
        Level = level;
        Ability = ability;
    }

    public override string ToString()
    {
        return $"{Name} (ID: {Id}, Type: {Type}, Level: {Level}, Ability: {Ability})";
    }
}