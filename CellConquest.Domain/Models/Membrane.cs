using System;
using System.Collections.Generic;
using System.Drawing;
using CellConquest.Domain.ValueObjects;

namespace CellConquest.Domain.Models;

public record Membrane
{
    public Wall Wall { get; }
    public List<PointF> Coordinates { get; }
    public string TouchedBy { get; init; }
    public bool IsTouched => TouchedBy != StaticGameValues.NoOne;

    public Membrane(Wall wall, List<PointF> coordinates, bool shouldBeMarkedAsOutline)
    {
        Wall = wall;
        Coordinates = coordinates;
        TouchedBy = StaticGameValues.NoOne;
        if (shouldBeMarkedAsOutline)
        {
            TouchedBy = StaticGameValues.Board;
        }
    }
}