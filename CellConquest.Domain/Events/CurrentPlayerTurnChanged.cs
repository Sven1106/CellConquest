using CellConquest.Domain.ValueObjects;
using CellConquest.Shared.Abstractions.Domain;

namespace CellConquest.Domain.Events;

public record CurrentPlayerTurnChanged(string PlayerName) : IDomainEvent;