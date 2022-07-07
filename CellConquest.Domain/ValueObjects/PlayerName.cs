using System.Collections.Generic;
using CellConquest.Domain.Exceptions;
using CellConquest.Domain.Models;

namespace CellConquest.Domain.ValueObjects;

public record PlayerName
{
    private readonly string _value;

    public PlayerName(string value)
    {
        var trimmedValue = value.Trim();
        if (string.IsNullOrWhiteSpace(trimmedValue))
        {
            throw new PlayerNameIsNullOrEmptyException();
        }

        if (StaticGameValues.Contains(trimmedValue))
        {
            throw new InvalidPlayerNameException();
        }

        _value = trimmedValue;
    }

    public static implicit operator string(PlayerName playerName) => playerName._value;
    public static implicit operator PlayerName(string playerName) => new(playerName);
}