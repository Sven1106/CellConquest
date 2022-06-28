using System;

namespace CellConquest.Domain.Exceptions;

public class OwnerCantBeRemovedException : ArgumentException
{
    public OwnerCantBeRemovedException(string message) : base(message)
    {
    }
}