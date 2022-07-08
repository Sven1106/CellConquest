using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CellConquest.Domain.ValueObjects;

namespace CellConquest.Domain.Models;

public record Cell
{
    // public Wall[] Id => Walls.OrderBy(x => x.First.X).ThenBy(x => x.First.Y).ThenBy(x => x.Second.X).ThenBy(x => x.Second.Y).ToArray(); // TODO refactor to walls sorting function.ToString();

    public List<PointF> Coordinates { get; }
    public string ConqueredBy { get; init; } = StaticGameValues.NoOne;
    public bool IsConquered => ConqueredBy != StaticGameValues.NoOne;

    public Cell(List<PointF> coordinates)
    {
        if (coordinates is null || coordinates.Any() == false)
        {
            throw new Exception();
        }
        Coordinates = coordinates;
    }
}