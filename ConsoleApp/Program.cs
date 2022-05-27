using System;
using CellConquest.Models;
using CellConquest.Services;

var board = new Board(Maps.Test);
RenderService.RenderBoardAsPng(board, "test");
Console.WriteLine("bordlayout");