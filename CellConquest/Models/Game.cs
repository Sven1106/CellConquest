using System.Collections.Generic;

namespace CellConquest.Models;

public class Game
{
    private Board Board { get; }
    public List<Player> Players { get; }

    public Game(GameConfig gameConfig)
    {
        Board = new Board(gameConfig.Outline);
    }
}