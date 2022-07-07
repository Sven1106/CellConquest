using CellConquest.Domain.Models;
using CellConquest.Domain.ValueObjects;
using CellConquest.Shared.Abstractions.Domain;

namespace CellConquest.Domain.Events;

public record GameCreated(string GameId, Board Board, string Owner) : IDomainEvent;