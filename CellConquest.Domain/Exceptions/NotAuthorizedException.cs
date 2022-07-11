using System;

namespace CellConquest.Domain.Exceptions;

public class NotAuthorizedException : ArgumentException
{
    public NotAuthorizedException(string message) : base(message)
    {
    }
}