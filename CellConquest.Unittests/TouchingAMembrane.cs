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

public class TouchingAMembrane
{
    private PointF[] TwoByTwo { get; } = { new(0, 0), new(2, 0), new(2, 2), new(0, 2) };
    //
    // [Fact]
    // public void ShouldSucceed()
    // {
    //     //GIVEN
    //     var gameConfig = new GameConfig("Test", "svend", TwoByTwo);
    //     var game = new Game(gameConfig);
    //     GameHelper.AddPlayerToGame(game, "steven");
    //     game = game with
    //     {
    //         GameState = GameState.Playing,
    //         CurrentPlayerTurn = "steven"
    //     };
    //     //WHEN
    //     RenderService.RenderBoardAsPng(game.Board, nameof(ShouldSucceed));
    //     game = GameHelper.TouchMembraneOnGame(game, "steven", "2");
    //     RenderService.RenderBoardAsPng(game.Board, nameof(ShouldSucceed));
    //     game = GameHelper.TouchMembraneOnGame(game, "svend", "3");
    //     RenderService.RenderBoardAsPng(game.Board, nameof(ShouldSucceed));
    //     game = GameHelper.TouchMembraneOnGame(game, "svend", "8");
    //     RenderService.RenderBoardAsPng(game.Board, nameof(ShouldSucceed));
    //     game = GameHelper.TouchMembraneOnGame(game, "svend", "7");
    //     //THEN
    //     RenderService.RenderBoardAsPng(game.Board, nameof(ShouldSucceed));
    // }

    [Fact]
    public void ShouldFailIfIfGameIsNotInPlayingPhase()
    {
        const string owner = "svend";
        const string playerName = "steven";
        var gameConfig = new GameConfig("Test", owner, TwoByTwo);
        var game = new Game(gameConfig);
        var firstMembrane = game.Board.Membranes.First();
        game = game with
        {
            CurrentPlayerTurn = playerName
        };
        var gameStates = Enum.GetValues(typeof(GameState)).Cast<GameState>();
        foreach (var gameState in gameStates.Where(x => x != GameState.Playing))
        {
            Assert.Throws<InvalidGameStateException>(() => GameHelper.TouchMembraneOnGame(game with { GameState = gameState }, playerName, firstMembrane.Wall));
        }
    }

    [Fact]
    public void ShouldFailIfIncorrectPlayerTurn()
    {
        const string owner = "svend";
        const string playerName = "steven";
        var gameConfig = new GameConfig("Test", owner, TwoByTwo);
        var game = new Game(gameConfig);
        var firstMembrane = game.Board.Membranes.First();
        game = game with
        {
            GameState = GameState.Playing,
            Players = game.Players.Add(playerName),
            CurrentPlayerTurn = owner
        };
        Assert.Throws<IncorrectPlayerTurnException>(() => GameHelper.TouchMembraneOnGame(game, playerName, firstMembrane.Wall));
    }

    [Fact]
    public void ShouldFailIfMembraneNotFound()
    {
        const string playerName = "steven";
        var gameConfig = new GameConfig("Test", "svend", TwoByTwo);
        var game = new Game(gameConfig);
        var wall = new Wall(new PointF(-1,-1), new PointF());
        game = game with
        {
            GameState = GameState.Playing,
            Players = game.Players.Add(playerName),
            CurrentPlayerTurn = playerName
        };

        Assert.Throws<MembraneNotFoundException>(() => GameHelper.TouchMembraneOnGame(game, playerName, wall));
    }

    [Fact]
    public void ShouldFailIfMembraneIsAlreadyTouched()
    {
        const string playerName = "steven";
        var gameConfig = new GameConfig("Test", "svend", TwoByTwo);
        var game = new Game(gameConfig);
        var modifiedMembrane = game.Board.Membranes.First();
        game = game with
        {
            GameState = GameState.Playing,
            Players = game.Players.Add(playerName),
            CurrentPlayerTurn = playerName,
            Board = game.Board with
            {
                Membranes = game.Board.Membranes.Replace(modifiedMembrane, modifiedMembrane with
                {
                    TouchedBy = playerName
                })
            }
        };

        Assert.Throws<MembraneAlreadyTouchedException>(() => GameHelper.TouchMembraneOnGame(game, playerName, modifiedMembrane.Wall));
    }

    [Fact]
    public void ShouldMarkItAsTouchedByPlayer()
    {
        const string playerName = "steven";
        var gameConfig = new GameConfig("Test", "svend", TwoByTwo);
        var game = new Game(gameConfig);
        var firstMembrane = game.Board.Membranes.First();
        game = game with
        {
            GameState = GameState.Playing,
            Players = game.Players.Add(playerName),
            CurrentPlayerTurn = playerName,
            Board = game.Board with
            {
                Membranes = game.Board.Membranes.Replace(firstMembrane, firstMembrane with
                {
                    TouchedBy = StaticGameValues.NoOne
                })
            }
        };

        game = GameHelper.TouchMembraneOnGame(game, playerName, firstMembrane.Wall);
        firstMembrane = game.Board.Membranes.First();
        Assert.True(firstMembrane.IsTouched);
        Assert.Equal(playerName, firstMembrane.TouchedBy);
    }

    [Fact]
    public void ShouldChangePlayerTurnIfNoCellWasConquered()
    {
        const string playerName = "steven";
        var gameConfig = new GameConfig("Test", "svend", TwoByTwo);
        var game = new Game(gameConfig);
        var firstMembrane = game.Board.Membranes.First();
        game = game with
        {
            GameState = GameState.Playing,
            Players = game.Players.Add(playerName),
            CurrentPlayerTurn = playerName,
            Board = game.Board with
            {
                Membranes = game.Board.Membranes.Replace(firstMembrane, firstMembrane with
                {
                    TouchedBy = StaticGameValues.NoOne
                })
            }
        };
        game = GameHelper.TouchMembraneOnGame(game, playerName, firstMembrane.Wall);
        Assert.NotEqual<PlayerName>(playerName, game.CurrentPlayerTurn);
    }

    [Fact]
    public void ShouldConquerACellIfAllConnectedMembranesAreTouched()
    {
        const string playerName = "steven";
        var gameConfig = new GameConfig("Test", "svend", TwoByTwo);
        var game = new Game(gameConfig);
        var secondMembrane = game.Board.Membranes[1];
        var thirdMembrane = game.Board.Membranes[2];
        game = game with
        {
            GameState = GameState.Playing,
            Players = game.Players.Add(playerName),
            CurrentPlayerTurn = playerName,
            Board = game.Board with
            {
                Membranes = game.Board.Membranes
                    .Replace(secondMembrane, secondMembrane with
                    {
                        TouchedBy = playerName
                    })
            }
        };

        game = GameHelper.TouchMembraneOnGame(game, playerName, thirdMembrane.Wall);
        var firstCell = game.Board.Cells.First();

        Assert.True(firstCell.IsConquered);
    }
}

public class ConqueringACell
{
    [Fact]
    public void ShouldMarkTheGameAsFinishedIfLastCell()
    {
        const string playerName = "steven";
        var gameConfig = new GameConfig("Test", "svend", new PointF[] { new(0, 0), new(2, 0), new(2, 1), new(0, 1) });
        var game = new Game(gameConfig);
        game = game with
        {
            CurrentPlayerTurn = playerName,
            GameState = GameState.Playing
        };
        var secondMembrane = game.Board.Membranes[1];
        game = GameHelper.TouchMembraneOnGame(game, playerName, secondMembrane.Wall);
        Assert.Equal(GameState.Finished, game.GameState);
    }
}