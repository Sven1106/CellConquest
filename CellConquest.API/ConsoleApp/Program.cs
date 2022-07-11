using System;
using System.Linq;
using CellConquest.Domain.Models;

var board = new Board(MapService.Maps.First(x => x.Name == "Test").Coordinates);
// RenderService.RenderBoardAsPng(board, "test");
Console.WriteLine("boardLayout");