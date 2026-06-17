using System;
using System.Collections.Generic;
using UnityEngine;

namespace GridBase
{
    public static class GridPathUtility
    {
        public static readonly Vector2Int[] CardinalDirections =
        {
            Vector2Int.right,
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.down
        };

        public static Vector2Int FilterDelta(Vector2Int delta, GridMoveAxis axis)
        {
            switch (axis)
            {
                case GridMoveAxis.Horizontal:
                    return new Vector2Int(delta.x, 0);
                case GridMoveAxis.Vertical:
                    return new Vector2Int(0, delta.y);
                case GridMoveAxis.Locked:
                    return Vector2Int.zero;
                default:
                    return delta;
            }
        }

        public static bool AreAdjacent(Vector2Int a, Vector2Int b)
        {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y) == 1;
        }

        public static IReadOnlyList<Vector2Int> BuildAxisStepPath(Vector2Int delta)
        {
            List<Vector2Int> steps = new List<Vector2Int>();
            int remainingX = Math.Abs(delta.x);
            int remainingY = Math.Abs(delta.y);
            int signX = Math.Sign(delta.x);
            int signY = Math.Sign(delta.y);

            while (remainingX > 0 || remainingY > 0)
            {
                if (remainingX == 0)
                {
                    steps.Add(new Vector2Int(0, signY));
                    remainingY--;
                }
                else if (remainingY == 0)
                {
                    steps.Add(new Vector2Int(signX, 0));
                    remainingX--;
                }
                else if (remainingX >= remainingY)
                {
                    steps.Add(new Vector2Int(signX, 0));
                    remainingX--;
                }
                else
                {
                    steps.Add(new Vector2Int(0, signY));
                    remainingY--;
                }
            }

            return steps;
        }
    }
}
