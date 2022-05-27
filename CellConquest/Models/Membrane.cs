using System;

namespace CellConquest.Models;

public class Membrane
{
    public string Id { get; }
    public string TouchedBy { get; private set; } = StaticGameValues.NoOne;
    public bool IsTouched => TouchedBy != StaticGameValues.NoOne;

    public Wall Wall { get; }

    public Membrane(string id, Wall wall, bool shouldBeMarkedAsOutline)
    {
        Id = id;
        Wall = wall;
        if (shouldBeMarkedAsOutline)
        {
            TouchedBy = StaticGameValues.Board;
        }
    }

    public void Touch(string playerName)
    {
        //  Check if membrane has already been touched.
        if (IsTouched)
        {
            throw new Exception($"Membrane with id: {Id} is already touched");
        }

        TouchedBy = playerName;

        // Emit MembraneTouched
    }
}