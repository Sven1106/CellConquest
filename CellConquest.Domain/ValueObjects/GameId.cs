using CellConquest.Domain.Exceptions;

namespace CellConquest.Domain.ValueObjects;

public record GameId
{
    private string Value { get; }

    private GameId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new GameIdIsNullOrEmptyException();
        }

        Value = value;
    }

    public static implicit operator GameId(string id) => new(id);
    public static implicit operator string(GameId gameId) => gameId.Value;
}