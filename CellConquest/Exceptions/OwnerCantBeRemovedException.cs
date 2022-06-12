using System;

namespace CellConquest.Exceptions;

public class OwnerCantBeRemovedException : ArgumentException
{
    public OwnerCantBeRemovedException(string message) : base(message)
    {
    }
}