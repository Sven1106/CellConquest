using System;
using System.Collections.Generic;
using CellConquest.Domain.ValueObjects;

namespace CellConquest.Domain.Models;

public record Membrane
{
    public Wall Wall { get; init; }
    public string Id { get; init; }
    public string TouchedBy { get; init; }
    public bool IsTouched => TouchedBy != StaticGameValues.NoOne;


    public List<Cell> Cells { get; init; }


    public Membrane(string id, Wall wall, bool shouldBeMarkedAsOutline)
    {
        Id = id;
        Wall = wall;
        TouchedBy = StaticGameValues.NoOne;
        if (shouldBeMarkedAsOutline)
        {
            TouchedBy = StaticGameValues.Board;
        }
    }
}