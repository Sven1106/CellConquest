using CellConquest.DTOs;
using CellConquest.Models;
using CellConquest.Services;
using Xunit;

namespace CellConquest.Unittests;

public class TouchingAMembrane
{
    [Fact]
    public void ShouldSucceed()
    {
        //GIVEN
        var gameConfig = new GameConfig("Test1", "svend", Maps.TwoByTwo);
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
}