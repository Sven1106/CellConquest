using System.Drawing;
using System.Linq;
using CellConquest.Domain.Exceptions;

namespace CellConquest.Domain.Models;

public record Cell
{
    public PointF[] Coordinates { get; }
    public string ConqueredBy { get; init; } = StaticGameValues.NoOne;
    public bool IsConquered => ConqueredBy != StaticGameValues.NoOne;

    public Cell(PointF[] coordinates)
    {
        if (coordinates is null || coordinates.Any() == false)
        {
            throw new CoordinatesAreNullOrEmptyException();
        }

        if (coordinates.Length != 4)
        {
            throw new CoordinatesInvalidException("Only four coordinates can be provided");
        }

        if (coordinates.Length != coordinates.Distinct().Count())
        {
            throw new CoordinatesInvalidException("No duplicates of coordinates are allowed");
        }

        Coordinates = coordinates;
    }
}