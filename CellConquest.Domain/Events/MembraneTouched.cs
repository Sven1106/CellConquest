using CellConquest.Shared.Abstractions.Domain;

namespace CellConquest.Domain.Events;

public record MembraneTouched(string wall, string PlayerName) : IDomainEvent;