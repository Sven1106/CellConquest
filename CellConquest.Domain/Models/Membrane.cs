using System.Drawing;
using System.Linq;
using CellConquest.Domain.Exceptions;

namespace CellConquest.Domain.Models;

public record Membrane
{
    public PointF[] Coordinates { get; }
    public string TouchedBy { get; init; }
    public bool IsTouched => TouchedBy != StaticGameValues.NoOne;

    public Membrane(PointF[] coordinates, bool shouldBeMarkedAsOutline)
    {
        if (coordinates is null || coordinates.Any() == false)
        {
            throw new CoordinatesAreNullOrEmptyException();
        }

        if (coordinates.Length != 2)
        {
            throw new CoordinatesInvalidException("Only two coordinates can be provided");
        }

        if (coordinates.Length != coordinates.Distinct().Count())
        {
            throw new CoordinatesInvalidException("No duplicates of coordinates are allowed");
        }

        Coordinates = coordinates;
        TouchedBy = StaticGameValues.NoOne;
        if (shouldBeMarkedAsOutline)
        {
            TouchedBy = StaticGameValues.Board;
        }
    }
}