using System;

namespace CellConquest.Exceptions;

public class MembraneAlreadyTouchedException : ArgumentException
{
    public MembraneAlreadyTouchedException(string message) : base(message)
    {
    }
}