using System.Collections.Generic;
using UnityEngine;

namespace GridBase
{
    public static class GridSnakeUtility
    {
        public static bool IsValidSnakeCells(IReadOnlyList<Vector2Int> cells)
        {
            if (cells == null || cells.Count == 0)
            {
                return false;
            }

            HashSet<Vector2Int> seen = new HashSet<Vector2Int>();
            for (int i = 0; i < cells.Count; i++)
            {
                if (!seen.Add(cells[i]))
                {
                    return false;
                }

                if (i > 0 && !GridPathUtility.AreAdjacent(cells[i - 1], cells[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
