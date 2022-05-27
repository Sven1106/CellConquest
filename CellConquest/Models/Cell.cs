using System;

namespace CellConquest.Models;

public class Cell
{
    public string Id { get; }
    public string ConqueredBy { get; private set; }

    public bool IsConquered => ConqueredBy != StaticGameValues.NoOne;

    public Cell(string id, string conqueredBy = StaticGameValues.NoOne)
    {
        Id = id;
        ConqueredBy = conqueredBy;
    }

    public void Conquer(string playerId)
    {
        //  Check if cell has already been conquered.
        if (IsConquered)
        {
            throw new Exception($"Cell with id: {Id} is already conquered");
        }

        ConqueredBy = playerId;
        // Emit Cell conquered
    }
}