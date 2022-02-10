namespace CellConquest.Models;

public class Player
{
    public string Name { get; }
    public string Color { get; }

    public Player(string name, string color)
    {
        Name = name;
        Color = color;
    }
}