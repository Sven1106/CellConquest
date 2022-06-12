using System.Drawing;
using CellConquest.DTOs;
using CellConquest.Exceptions;
using CellConquest.Models;
using CellConquest.Services;
using Xunit;

namespace CellConquest.Unittests;

public class StartingAGame
{
    private PointF[] TwoByTwo { get; } = { new(0, 0), new(2, 0), new(2, 2), new(0, 2), };

    [Fact]
    public void ShouldFailIfInsufficientNumberOfPlayers()
    {
        var gameConfig = new GameConfig("Test", "svend", TwoByTwo);
        var game = new Game(gameConfig);
        Assert.Throws<InsufficientPlayersException>(() => GameService.StartGame(game));
    }

    [Fact]
    public void ShouldSucceedIfSufficientNumberOfPlayers()
    {
        var gameConfig = new GameConfig("Test", "svend", TwoByTwo);
        var game = new Game(gameConfig);
        game = game with
        {
            Players = game.Players.Add("steven")
        };
        GameService.StartGame(game);
    }
}