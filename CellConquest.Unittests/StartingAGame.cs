using System;
using System.Drawing;
using System.Linq;
using CellConquest.Domain.Entities;
using CellConquest.Domain.Exceptions;
using CellConquest.Domain.Helpers;
using CellConquest.Domain.Models;
using CellConquest.Domain.Services;
using CellConquest.Domain.ValueObjects;
using Xunit;

namespace CellConquest.Unittests;

public class StartingAGame
{
    private PointF[] TwoByTwo { get; } = { new(0, 0), new(2, 0), new(2, 2), new(0, 2) };

    [Fact]
    public void ShouldFailIfIfGameIsNotInSetupPhase()
    {
        const string owner = "svend";
        const string playerName = "steven";
        var gameConfig = new GameConfig("Test", owner, TwoByTwo);
        var game = new Game(gameConfig);
        game = game with
        {
            Players = game.Players.Add(playerName)
        };
        var gameStates = Enum.GetValues(typeof(GameState)).Cast<GameState>();
        foreach (var gameState in gameStates.Where(x => x != GameState.WaitForPlayers))
        {
            Assert.Throws<InvalidGameStateException>(() => GameHelper.StartGame(game with
            {
                GameState = gameState
            }, owner));
        }
    }

    [Fact]
    public void ShouldFailIfNotAdmin()
    {
        const string playerName = "steven";
        var gameConfig = new GameConfig("Test", "svend", TwoByTwo);
        var game = new Game(gameConfig);
        game = game with
        {
            Players = game.Players.Add(playerName)
        };
        Assert.Throws<NotAuthorizedException>(() => GameHelper.StartGame(game, playerName));
    }

    [Fact]
    public void ShouldFailIfInsufficientNumberOfPlayers()
    {
        const string owner = "svend";
        var gameConfig = new GameConfig("Test", owner, TwoByTwo);
        var game = new Game(gameConfig);
        Assert.Throws<InsufficientPlayersException>(() => GameHelper.StartGame(game, owner));
    }

    [Fact]
    public void ShouldSucceedIfSufficientNumberOfPlayers()
    {
        const string owner = "svend";
        const string playerName = "steven";
        var gameConfig = new GameConfig("Test", owner, TwoByTwo);
        var game = new Game(gameConfig);
        game = game with
        {
            Players = game.Players.Add(playerName)
        };
        GameHelper.StartGame(game, owner);
    }
}