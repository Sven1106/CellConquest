using System;

namespace CellConquest.Exceptions;

public class MembraneNotFoundException : ArgumentException
{
    public MembraneNotFoundException(string message) : base(message)
    {
    }
}