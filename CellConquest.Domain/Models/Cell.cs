using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CellConquest.Domain.ValueObjects;

namespace CellConquest.Domain.Models;

public record Cell
{
    public Wall[] Id => Walls.OrderBy(x => x.First.X).ThenBy(x => x.First.Y).ThenBy(x => x.Second.X).ThenBy(x => x.Second.Y).ToArray(); // TODO refactor to walls sorting function.ToString();

    public Wall[] Walls { get; }
    public List<PointF> Coordinates { get; }
    public string ConqueredBy { get; init; } = StaticGameValues.NoOne;
    public bool IsConquered => ConqueredBy != StaticGameValues.NoOne;

    public Cell(Wall[] walls, List<PointF> coordinates)
    {
        if (walls is null || walls.Any() == false)
        {
            throw new Exception();
        }

        Walls = walls;
        Coordinates = coordinates;
    }
}