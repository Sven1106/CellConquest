using CellConquest.Domain.Models;
using CellConquest.Shared.Abstractions.Domain;

namespace CellConquest.Domain.Events;

public record GameStateChangedEvent(GameState GameState) : IDomainEvent;