using System.Collections.Generic;
using System.Drawing;

namespace CellConquest.Domain.Models;

public class Map
{
    public string Name { get; }
    public PointF[] Coordinates { get; }

    public Map(string name, PointF[] coordinates)
    {
        Name = name;
        Coordinates = coordinates;
    }
}

public static class MapService
{
    public static IEnumerable<Map> Maps { get; } = new List<Map>
    {
        new("Test", new PointF[]
        {
            new(1, 1),
            new(5, 1),
            new(7, 4),
            new(9, 4),
            new(10, 1),
            new(3, 4),
            new(8, 9),
            new(1, 9)
        }),
        new("Test2", new PointF[]
        {
            new(0, 2),
            new(1, 1),
            new(3, 1),
            new(4, 2),
            new(2, 2),
            new(3, 3),
            new(1, 3)
        }),
        new("Test3", new PointF[]
        {
            new(0, 0),
            new(1, 0),
            new(1, 1),
            new(2, 1),
            new(2, 0),
            new(3, 0),
            new(3, 3),
            new(0, 3)
        }),
        new("Diamond", new PointF[]
        {
            new(4, 0),
            new(8, 4),
            new(4, 8),
            new(0, 4)
        }),
        new("TwoByTwo", new PointF[]
        {
            new(0, 0),
            new(2, 0),
            new(2, 2),
            new(0, 2)
        }),
        new("ThreeByThree", new PointF[]
        {
            new(0, 0),
            new(3, 0),
            new(3, 3),
            new(0, 3)
        }),
        new("FiveByFive", new PointF[]
        {
            new(0, 0),
            new(5, 0),
            new(5, 5),
            new(0, 5)
        }),
        new("Mario", new PointF[]
        {
            new(3, 0),
            new(8, 0),
            new(8, 1),
            new(11, 1),
            new(11, 2),
            new(9, 2),
            new(9, 3),
            new(11, 3),
            new(11, 4),
            new(12, 4),
            new(12, 5),
            new(11, 5),
            new(11, 6),
            new(9, 6),
            new(9, 7),
            new(10, 7),
            new(10, 8),
            new(11, 8),
            new(11, 9),
            new(12, 9),
            new(12, 13),
            new(10, 13),
            new(10, 14),
            new(11, 14),
            new(11, 15),
            new(12, 15),
            new(12, 16),
            new(8, 16),
            new(8, 14),
            new(7, 14),
            new(7, 13),
            new(5, 13),
            new(5, 14),
            new(4, 14),
            new(4, 16),
            new(0, 16),
            new(0, 15),
            new(1, 15),
            new(1, 14),
            new(2, 14),
            new(2, 13),
            new(0, 13),
            new(0, 9),
            new(1, 9),
            new(1, 8),
            new(2, 8),
            new(2, 7),
            new(3, 7),
            new(3, 6),
            new(2, 6),
            new(2, 5),
            new(1, 5),
            new(1, 3),
            new(2, 3),
            new(2, 1),
            new(3, 1)
        })
    };
}