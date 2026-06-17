using System.Collections.Generic;
using UnityEngine;

namespace GridBase
{
    public readonly struct GridCellValidation
    {
        public GridCellValidation(bool isValid, GridFailReason reason, IReadOnlyList<Vector2Int> invalidCells)
        {
            IsValid = isValid;
            Reason = reason;
            InvalidCells = invalidCells ?? new List<Vector2Int>();
        }

        public bool IsValid { get; }
        public GridFailReason Reason { get; }
        public IReadOnlyList<Vector2Int> InvalidCells { get; }
    }
}
