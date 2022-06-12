using System.Drawing;
using System.Linq;
using CellConquest.DTOs;
using CellConquest.Exceptions;
using CellConquest.Models;
using CellConquest.Services;
using Xunit;

namespace CellConquest.Unittests;

public class TouchingAMembrane
{
    private PointF[] TwoByTwo { get; } = { new(0, 0), new(2, 0), new(2, 2), new(0, 2), };

    [Fact]
    public void ShouldSucceed()
    {
        //GIVEN
        var gameConfig = new GameConfig("Test", "svend", TwoByTwo);
        var game = new Game(gameConfig);
        GameService.AddPlayerToGame(game, "steven");
        game = game with { CurrentPlayerTurn = "steven" };
        //WHEN
        RenderService.RenderBoardAsPng(game.Board, nameof(ShouldSucceed));
        game = GameService.TouchMembraneOnGame(game, "steven", "2");
        RenderService.RenderBoardAsPng(game.Board, nameof(ShouldSucceed));
        game = GameService.TouchMembraneOnGame(game, "svend", "3");
        RenderService.RenderBoardAsPng(game.Board, nameof(ShouldSucceed));
        game = GameService.TouchMembraneOnGame(game, "svend", "8");
        RenderService.RenderBoardAsPng(game.Board, nameof(ShouldSucceed));
        game = GameService.TouchMembraneOnGame(game, "svend", "7");
        //THEN
        RenderService.RenderBoardAsPng(game.Board, nameof(ShouldSucceed));
    }

    [Fact]
    public void ShouldFailIfIncorrectPlayerTurn()
    {
        const string owner = "svend";
        const string playerName = "steven";
        var gameConfig = new GameConfig("Test", owner, TwoByTwo);
        var game = new Game(gameConfig);
        game = game with
        {
            Players = game.Players.Add(playerName),
            CurrentPlayerTurn = owner,
        };
        Assert.Throws<IncorrectPlayerTurnException>(() => GameService.TouchMembraneOnGame(game, playerName, "2"));
    }

    [Fact]
    public void ShouldFailIfMembraneNotFound()
    {
        const string playerName = "steven";
        var gameConfig = new GameConfig("Test", "svend", TwoByTwo);
        var game = new Game(gameConfig);
        const string membraneId = "test";
        game = game with
        {
            Players = game.Players.Add(playerName),
            CurrentPlayerTurn = playerName
        };

        Assert.Throws<MembraneNotFoundException>(() => GameService.TouchMembraneOnGame(game, playerName, membraneId));
    }

    [Fact]
    public void ShouldFailIfMembraneIsAlreadyTouched()
    {
        const string playerName = "steven";
        var gameConfig = new GameConfig("Test", "svend", TwoByTwo);
        var game = new Game(gameConfig);
        var modifiedMembrane = game.Board.Membranes.FirstOrDefault();
        game = game with
        {
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

        Assert.Throws<MembraneAlreadyTouchedException>(() => GameService.TouchMembraneOnGame(game, playerName, modifiedMembrane.Id));
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

        game = GameService.TouchMembraneOnGame(game, playerName, firstMembrane.Id);
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
        game = GameService.TouchMembraneOnGame(game, playerName, firstMembrane.Id);
        Assert.NotEqual(playerName, game.CurrentPlayerTurn);
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

        game = GameService.TouchMembraneOnGame(game, playerName, thirdMembrane.Id);
        var firstCell = game.Board.Cells.First();

        Assert.True(firstCell.IsConquered);
    }
}

public class ConqueringACell
{
    //  ShouldMarkTheGameAsFinishedIfLastCell
}