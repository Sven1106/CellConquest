using System;
using System.Drawing;
using System.IO;
using System.Linq;
using CellConquest.Models;
using PointF = System.Drawing.PointF;

var board = new Board(Maps.Test);
const int width = 1000;
const int height = 1000;
const int scale = 35;


// Create a drawing target
Bitmap bitmap = new(width, height);

var graphics = Graphics.FromImage(bitmap);
var outline = board.Outline.Select(point => new PointF(point.X * scale, point.Y * scale)).ToArray();
var scaledOutline = PolygonHelper.GetEnlargedPolygon(
    board.Outline.Select(point => new PointF(point.X * scale, point.Y * scale)).ToArray(), scale * 0.025F);

graphics.DrawPolygon(Pens.Red, scaledOutline);
foreach (var cell in board.Cells)
{
    foreach (var membrane in cell.Membranes)
    {
        var color = membrane.TouchedBy switch
        {
            StaticGameValues.Board => new Pen(Color.Black, 3),
            StaticGameValues.NoOne => new Pen(Color.LightGray, 3),
            _ => new Pen(Color.Coral, 3)
        };
        graphics.DrawLine(
            color,
            new PointF(membrane.P1.X * scale, membrane.P1.Y * scale),
            new PointF(membrane.P2.X * scale, membrane.P2.Y * scale)
        );
    }
}

graphics.DrawPolygon(Pens.Black, outline);
// Save
bitmap.Save(Path.Combine(Directory.GetCurrentDirectory(), "img.png"));
Console.WriteLine("bordlayout");