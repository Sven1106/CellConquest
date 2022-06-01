using System.Drawing;
using System.IO;
using System.Linq;
using CellConquest.Helpers;
using CellConquest.Models;

namespace CellConquest.Services;

public static class RenderService
{
    public static void RenderBoardAsPng(Board board, string imageName)
    {
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
            var cellMembranes = board.CellMembranes.Where(x => x.CellId == cell.Id);
            foreach (var cellMembrane in cellMembranes)
            {
                var membrane = board.Membranes.FirstOrDefault(x => x.Id == cellMembrane.MembraneId);
                var color = membrane.TouchedBy switch
                {
                    StaticGameValues.Board => new Pen(Color.Black, 3),
                    StaticGameValues.NoOne => new Pen(Color.LightGray, 3),
                    _ => new Pen(Color.Coral, 3)
                };
                graphics.DrawLine(
                    color,
                    new PointF(membrane.Wall.Point1.X * scale, membrane.Wall.Point1.Y * scale),
                    new PointF(membrane.Wall.Point2.X * scale, membrane.Wall.Point2.Y * scale)
                );
            }
        }

        graphics.DrawPolygon(Pens.Black, outline);
        // Save
        bitmap.Save(Path.Combine(Directory.GetCurrentDirectory(), $"{imageName}.png"));
    }
}