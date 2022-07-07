using CellConquest.Domain.ValueObjects;
using CellConquest.Shared.Abstractions.Domain;

namespace CellConquest.Domain.Events;

public record PlayerRemoved(string PlayerName) : IDomainEvent;