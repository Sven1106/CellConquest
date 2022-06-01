using System;

namespace CellConquest.Models;

public record Membrane
{
    public string Id { get; init; }
    public string TouchedBy { get; init; } = StaticGameValues.NoOne;
    public bool IsTouched => TouchedBy != StaticGameValues.NoOne;

    public Wall Wall { get; init; }

    public Membrane(string id, Wall wall, bool shouldBeMarkedAsOutline)
    {
        Id = id;
        Wall = wall;
        if (shouldBeMarkedAsOutline)
        {
            TouchedBy = StaticGameValues.Board;
        }
    }
}