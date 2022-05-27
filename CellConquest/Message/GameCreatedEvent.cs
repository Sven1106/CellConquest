using System;
using Paramore.Brighter;

namespace CellConquest.Message;

public record GameCreatedEvent(Guid Id, string GameId) : IRequest
{
    public Guid Id { get; set; } = Id;
}