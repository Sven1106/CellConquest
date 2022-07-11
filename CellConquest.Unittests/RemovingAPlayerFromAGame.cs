using System;
using System.Drawing;
using System.Linq;
using CellConquest.Domain.Exceptions;
using CellConquest.Domain.Helpers;
using CellConquest.Domain.Models;
using CellConquest.Domain.ValueObjects;
using Xunit;

namespace CellConquest.Unittests;

public class RemovingAPlayerFromAGame
{
    private PointF[] TwoByTwo { get; } = { new(0, 0), new(2, 0), new(2, 2), new(0, 2) };

    [Fact]
    public void ShouldFailIfPlayerDoesntExist()
    {
        const string playerName = "steven";
        var gameConfig = new GameConfig("Test", "svend", TwoByTwo);
        var game = new Game(gameConfig);
        Assert.Throws<PlayerNotFoundException>(() => GameHelper.RemovePlayerFromGame(game, playerName));
    }

    [Fact]
    public void ShouldFailIfPlayerIsOwnerOfGame()
    {
        const string playerName = "steven";
        var gameConfig = new GameConfig("Test", playerName, TwoByTwo);
        var game = new Game(gameConfig);
        Assert.Throws<OwnerCantBeRemovedException>(() => GameHelper.RemovePlayerFromGame(game, playerName));
    }

    [Fact]
    public void ShouldFailIfGameIsNotInSetupPhase()
    {
        const string playerName = "steven";
        var gameConfig = new GameConfig("Test", "svend", TwoByTwo);
        var game = new Game(gameConfig);
        game = game with
        {
            Players = game.Players.Add(playerName)
        };
        var gameStates = Enum.GetValues(typeof(GameState)).Cast<GameState>();
        foreach (var gameState in gameStates.Where(x => x != GameState.WaitForPlayers))
        {
            Assert.Throws<InvalidGameStateException>(() => GameHelper.RemovePlayerFromGame(game with { GameState = gameState }, playerName));
        }
    }

    [Fact]
    public void ShouldSucceedIfPlayerNameIsLegal()
    {
        const string playerName = "steven";
        var gameConfig = new GameConfig("Test", "svend", TwoByTwo);
        var game = new Game(gameConfig);
        game = game with
        {
            Players = game.Players.Add(playerName)
        };

        game = GameHelper.RemovePlayerFromGame(game, playerName);
        // Assert.DoesNotContain(playerName, game.Players);
    }
}