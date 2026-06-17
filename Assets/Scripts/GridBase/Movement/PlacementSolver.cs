using System.Collections.Generic;
using UnityEngine;

namespace GridBase
{
    public class PlacementSolver
    {
        private readonly GridMap _map;
        private readonly IGridShapeProvider _shapeProvider;

        public PlacementSolver(GridMap map, IGridShapeProvider shapeProvider)
        {
            _map = map;
            _shapeProvider = shapeProvider;
        }

        public GridMoveResult Preview(string shapeId, Vector2Int pivot, int rotation)
        {
            if (!_shapeProvider.TryGetShape(shapeId, out GridShape shape))
            {
                return GridMoveResult.Failed(GridFailReason.InvalidShape);
            }

            IReadOnlyList<Vector2Int> cells = shape.GetWorldCells(pivot, rotation);
            GridCellValidation validation = _map.ValidateCells(cells);
            return GridMoveResult.Preview(validation.IsValid, validation.Reason, pivot, cells, validation.InvalidCells);
        }

        public GridFailReason GetPlacementFailure(string shapeId, Vector2Int pivot, int rotation)
        {
            if (!_shapeProvider.TryGetShape(shapeId, out GridShape shape))
            {
                return GridFailReason.InvalidShape;
            }

            GridCellValidation validation = _map.ValidateCells(shape.GetWorldCells(pivot, rotation));
            return validation.IsValid ? GridFailReason.None : validation.Reason;
        }
    }
}
