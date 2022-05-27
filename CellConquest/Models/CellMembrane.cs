namespace CellConquest.Models;

public class CellMembrane
{
    public Cell Cell { get; }
    public Membrane Membrane { get; }

    public CellMembrane(Cell cell, Membrane membrane)
    {
        Cell = cell;
        Membrane = membrane;
    }
}