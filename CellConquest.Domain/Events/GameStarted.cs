using CellConquest.Shared.Abstractions.Domain;

namespace CellConquest.Domain.Events;

public record GameStarted(string PlayerName) : IDomainEvent;