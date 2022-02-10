using System;
using System.Collections.Generic;
using System.Drawing;

namespace CellConquest.Models;

public class Membrane
{
    public Guid Id { get; } = Guid.NewGuid();
    public string TouchedBy { get; } = StaticGameValues.NoOne;
    public System.Drawing.PointF P1 { get; }
    public System.Drawing.PointF P2 { get; }
    public List<Cell> Cells { get; } = new();

    public Membrane(System.Drawing.PointF p1, System.Drawing.PointF p2, bool isOnOutline) //TODO should the coordinate aspect get removed?
    {
        P1 = p1;
        P2 = p2;
        if (isOnOutline)
        {
            TouchedBy = StaticGameValues.Board;
        }
    }
}