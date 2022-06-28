using System;
using CellConquest.Domain.Models;
using CellConquest.Domain.Services;

var board = new Board(Maps.Test);
RenderService.RenderBoardAsPng(board, "test");
Console.WriteLine("boardLayout");