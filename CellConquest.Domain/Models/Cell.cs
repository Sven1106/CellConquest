using System;

namespace CellConquest.Domain.Models;

public record Cell(string Id, string ConqueredBy = StaticGameValues.NoOne)
{
    public bool IsConquered => ConqueredBy != StaticGameValues.NoOne;
}