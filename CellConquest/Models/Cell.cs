using System;
using System.Collections.Generic;

namespace CellConquest.Models;

public class Cell
{
    public Guid Id { get; } = Guid.NewGuid();
    public string ConqueredBy { get; set; } = StaticGameValues.NoOne;
    public List<Membrane> Membranes { get; } = new();
}