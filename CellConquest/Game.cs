using System;

namespace CellConquest
{
    public static class StaticGameValues
    {
        public const string NoOne = "noOne";
        public const string Board = "board";
    }

    public class Cell
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string ConqueredBy { get; private set; } = StaticGameValues.NoOne;
    }


}