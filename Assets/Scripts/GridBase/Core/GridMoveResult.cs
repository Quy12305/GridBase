using System.Collections.Generic;
using UnityEngine;

namespace GridBase
{
    public class GridMoveResult
    {
        public static GridMoveResult Failed(GridFailReason reason)
        {
            return new GridMoveResult(false, reason);
        }

        public static GridMoveResult Succeeded(
            Vector2Int pivot,
            IReadOnlyList<Vector2Int> cells,
            IReadOnlyList<Vector2Int> invalidCells = null)
        {
            return new GridMoveResult(true, GridFailReason.None)
            {
                Pivot = pivot,
                Cells = Copy(cells),
                InvalidCells = Copy(invalidCells)
            };
        }

        public static GridMoveResult Preview(
            bool isValid,
            GridFailReason reason,
            Vector2Int pivot,
            IReadOnlyList<Vector2Int> cells,
            IReadOnlyList<Vector2Int> invalidCells = null)
        {
            return new GridMoveResult(isValid, isValid ? GridFailReason.None : reason)
            {
                Pivot = pivot,
                Cells = Copy(cells),
                InvalidCells = Copy(invalidCells)
            };
        }

        private GridMoveResult(bool success, GridFailReason reason)
        {
            Success = success;
            Reason = reason;
            Cells = new List<Vector2Int>();
            InvalidCells = new List<Vector2Int>();
        }

        public bool Success { get; }
        public GridFailReason Reason { get; }
        public Vector2Int Pivot { get; private set; }
        public IReadOnlyList<Vector2Int> Cells { get; private set; }
        public IReadOnlyList<Vector2Int> InvalidCells { get; private set; }

        private static IReadOnlyList<Vector2Int> Copy(IReadOnlyList<Vector2Int> cells)
        {
            return cells == null ? new List<Vector2Int>() : new List<Vector2Int>(cells);
        }
    }
}
