using System;

namespace CellConquest.Domain.Exceptions;

public class MembraneNotFoundException : ArgumentException
{
    public MembraneNotFoundException(string message) : base(message)
    {
    }
}