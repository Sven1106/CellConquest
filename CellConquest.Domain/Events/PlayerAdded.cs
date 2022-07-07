using CellConquest.Domain.ValueObjects;
using CellConquest.Shared.Abstractions.Domain;

namespace CellConquest.Domain.Events;

public record PlayerAdded(string PlayerName) : IDomainEvent;