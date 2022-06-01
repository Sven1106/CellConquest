using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using CellConquest.Helpers;

namespace CellConquest.Models;

public record Board
{
    public PointF[] Outline { get; }
    public ImmutableList<Cell> Cells { get; init; }
    public ImmutableList<Membrane> Membranes { get; init; }
    public ImmutableList<CellMembrane> CellMembranes { get; init; }

    public Board(PointF[] outline)
    {
        Outline = outline;
        var (cells, membranes, cellMembranes) = BoardHelper.CreateCellsFromPolygon(Outline);
        Cells = ImmutableList<Cell>.Empty.AddRange(cells);
        Membranes = ImmutableList<Membrane>.Empty.AddRange(membranes);
        CellMembranes = ImmutableList<CellMembrane>.Empty.AddRange(cellMembranes);
    }
}