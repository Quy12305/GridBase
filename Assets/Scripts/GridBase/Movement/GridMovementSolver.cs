using System.Collections.Generic;
using UnityEngine;

namespace GridBase
{
    public class GridMovementSolver
    {
        private readonly GridMap _map;
        private readonly IGridShapeProvider _shapeProvider;

        public GridMovementSolver(GridMap map, IGridShapeProvider shapeProvider)
        {
            _map = map;
            _shapeProvider = shapeProvider;
        }

        public GridMoveResult SolveBlockMove(string objectId, Vector2Int startPointerCell, Vector2Int targetPointerCell)
        {
            if (!_map.TryGetObject(objectId, out GridObject gridObject) || gridObject.Kind == GridObjectKind.Snake)
            {
                return GridMoveResult.Failed(GridFailReason.InvalidObject);
            }

            if (gridObject.MoveAxis == GridMoveAxis.Locked)
            {
                return GridMoveResult.Failed(GridFailReason.MoveLocked);
            }

            if (!_shapeProvider.TryGetShape(gridObject.ShapeId, out GridShape shape))
            {
                return GridMoveResult.Failed(GridFailReason.InvalidShape);
            }

            Vector2Int filteredDelta = GridPathUtility.FilterDelta(targetPointerCell - startPointerCell, gridObject.MoveAxis);
            if (filteredDelta == Vector2Int.zero)
            {
                return GridMoveResult.Succeeded(gridObject.Pivot, gridObject.GetOccupiedCells(_shapeProvider));
            }

            Vector2Int targetPivot = gridObject.Pivot + filteredDelta;
            IReadOnlyList<Vector2Int> targetCells = shape.GetWorldCells(targetPivot, gridObject.Rotation);
            GridCellValidation validation = _map.ValidateCells(targetCells, objectId);
            return GridMoveResult.Preview(
                validation.IsValid,
                validation.Reason,
                targetPivot,
                targetCells,
                validation.InvalidCells);
        }
    }
}
