using System;

namespace GridBase
{
    public enum GridObjectKind
    {
        Snake,
        Block,
        PlaceableItem
    }

    public enum GridMoveAxis
    {
        Free,
        Horizontal,
        Vertical,
        Locked
    }

    public enum GridDragHandle
    {
        None,
        Head,
        Tail,
        Body
    }

    public enum GridFailReason
    {
        None,
        InvalidMap,
        InvalidObject,
        InvalidShape,
        InvalidCell,
        InvalidDragHandle,
        InvalidDirection,
        MoveLocked,
        OutOfBounds,
        BlockedCell,
        OccupiedCell,
        NoValidMove
    }

    public enum GridDragMode
    {
        None,
        SnakeHead,
        SnakeTail,
        Block
    }

    public static class GridEnumParser
    {
        public static T ParseOrDefault<T>(string value, T fallback) where T : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            return Enum.TryParse(value, true, out T parsed) ? parsed : fallback;
        }
    }
}
