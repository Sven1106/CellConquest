using System;

namespace CellConquest.Domain.Exceptions;

public class MembraneAlreadyTouchedException : ArgumentException
{
    public MembraneAlreadyTouchedException(string message) : base(message)
    {
    }
}