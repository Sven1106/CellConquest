using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CellConquest.Helpers;

namespace CellConquest.Models;

public class Board
{
    // public readonly ImmutableDictionary<string, ImmutableList<Cell>> CellsByMembraneId;
    //
    // public readonly ImmutableDictionary<string, Membrane> MembraneByMembraneId;

    // private readonly Dictionary<string, ImmutableList<Membrane>> _membranesByCellId;
    public PointF[] Outline { get; }
    public List<Cell> Cells { get; }
    public List<Membrane> Membranes { get; }
    public List<CellMembrane> CellMembranes { get; }

    public Board(PointF[] outline)
    {
        Outline = outline;
        var (cells, membranes, cellMembranes) = BoardHelper.CreateCellsFromPolygon(Outline);
        Cells = cells;
        Membranes = membranes;
        CellMembranes = cellMembranes;
        // CellsByMembraneId = BoardHelper.CreateCellsByMembraneIdLookUpTable(Cells);
        // MembraneByMembraneId = BoardHelper.CreateMembraneByMembraneIdLookUpTable(Cells);
        // _membranesByCellId = Cells.ToDictionary(x => x.Id, x => x.Membranes);
    }

    public List<Cell> GetConquerableCellsConnectedToMembraneId(string membraneId)
    {
        return CellMembranes.Where(cellMembrane
                => cellMembrane.Membrane.Id == membraneId
                   && cellMembrane.Cell.IsConquered == false
                   && CellMembranes.Where(cellMembrane2
                           => cellMembrane2.Cell.Id == cellMembrane.Cell.Id)
                       .All(x => x.Membrane.IsTouched)
            ).Select(cellMembrane => cellMembrane.Cell)
            .ToList();
    }

    public Membrane? GetMembraneById(string membraneId)
    {
        return Membranes.FirstOrDefault(x => x.Id == membraneId);
    }

    public List<Cell> GetCellsConnectedToMembraneId(string membraneId)
    {
        return CellMembranes.Where(x => x.Membrane.Id == membraneId).Select(x => x.Cell).ToList();
    }
    
}