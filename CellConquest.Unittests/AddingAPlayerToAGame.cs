using System;
using System.Drawing;
using System.Linq;
using CellConquest.Domain.Entities;
using CellConquest.Domain.Exceptions;
using CellConquest.Domain.Helpers;
using CellConquest.Domain.Models;
using CellConquest.Domain.ValueObjects;
using Xunit;

namespace CellConquest.Unittests;

public class AddingAPlayerToAGame
{
    private PointF[] TwoByTwo { get; } = { new(0, 0), new(2, 0), new(2, 2), new(0, 2) };

    [Fact]
    public void ShouldFailIfPlayerAlreadyExist()
    {
        const string playerName = "svend";
        var gameConfig = new GameConfig("Test", playerName, TwoByTwo);
        var game = new Game(gameConfig);


        Assert.Throws<PlayerAlreadyExistException>(() => GameHelper.AddPlayerToGame(game, playerName));
    }

    [Fact]
    public void ShouldFailIfGameIsNotInSetupPhase()
    {
        const string playerName = "steven";
        var gameConfig = new GameConfig("Test", "svend", TwoByTwo);
        var game = new Game(gameConfig);
        var gameStates = Enum.GetValues(typeof(GameState)).Cast<GameState>();
        foreach (var gameState in gameStates.Where(x => x != GameState.WaitForPlayers))
        {
            Assert.Throws<InvalidGameStateException>(() => GameHelper.AddPlayerToGame(game with { GameState = gameState }, playerName));
        }
    }

    [Fact]
    public void ShouldFailIfPlayerNameIsReservedBySystem()
    {
        var gameConfig = new GameConfig("Test", "svend", TwoByTwo);
        var game = new Game(gameConfig);
        Assert.Throws<InvalidPlayerNameException>(() => GameHelper.AddPlayerToGame(game, StaticGameValues.Board));
        Assert.Throws<InvalidPlayerNameException>(() => GameHelper.AddPlayerToGame(game, StaticGameValues.NoOne));
    }

    [Fact]
    public void ShouldSucceedIfPlayerNameIsLegal()
    {
        const string playerName = "steven";
        var gameConfig = new GameConfig("Test", "svend", TwoByTwo);
        var game = new Game(gameConfig);
        game = GameHelper.AddPlayerToGame(game, playerName);
        Assert.Contains<PlayerName>(playerName, game.Players);
    }
}