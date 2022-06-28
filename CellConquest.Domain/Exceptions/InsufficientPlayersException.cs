using System;

namespace CellConquest.Domain.Exceptions;

public class InsufficientPlayersException : ArgumentException
{
    public InsufficientPlayersException(string message) : base(message)
    {
    }
}
public class NotAuthorizedException : ArgumentException
{
    public NotAuthorizedException(string message) : base(message)
    {
    }
}