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
        game.AddPlayer("steven");
        game.Start();
        //WHEN
        RenderService.RenderBoardAsPng(game.Board, nameof(ShouldSucceed));
        game.TouchMembrane("steven", "2");
        RenderService.RenderBoardAsPng(game.Board, nameof(ShouldSucceed));
        game.TouchMembrane("svend", "3");
        RenderService.RenderBoardAsPng(game.Board, nameof(ShouldSucceed));
        game.TouchMembrane("svend", "8");
        RenderService.RenderBoardAsPng(game.Board, nameof(ShouldSucceed));
        game.TouchMembrane("svend", "7");
        //THEN

        RenderService.RenderBoardAsPng(game.Board, nameof(ShouldSucceed));
    }
}