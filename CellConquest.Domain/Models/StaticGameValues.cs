using System.Linq;
using System.Reflection;

namespace CellConquest.Domain.Models;

public static class StaticGameValues
{
    public const string NoOne = "noOne";
    public const string Board = "board";

    public static bool Contains(string value)
    {
        var doesContain = typeof(StaticGameValues).GetFields(BindingFlags.Static | BindingFlags.Public)
            .Where(x => x.IsLiteral && !x.IsInitOnly)
            .Select(x => x.GetValue(null)).Cast<string>().Contains(value);
        return doesContain;
    }
}

public enum GameValues
{
    NoOne,
    Board
}