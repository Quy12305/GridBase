using System.Collections.Generic;
using UnityEngine;

namespace GridBase
{
    public class GridMap
    {
        private readonly HashSet<Vector2Int> _blockedCells = new HashSet<Vector2Int>();
        private readonly Dictionary<string, GridObject> _objects = new Dictionary<string, GridObject>();
        private readonly Dictionary<Vector2Int, string> _occupancy = new Dictionary<Vector2Int, string>();

        public GridMap(int width, int height, IEnumerable<Vector2Int> blockedCells = null)
        {
            Width = width;
            Height = height;

            if (blockedCells == null)
            {
                return;
            }

            foreach (Vector2Int cell in blockedCells)
            {
                if (IsInside(cell))
                {
                    _blockedCells.Add(cell);
                }
            }
        }

        public int Width { get; }
        public int Height { get; }
        public IReadOnlyDictionary<string, GridObject> Objects => _objects;
        public IReadOnlyCollection<Vector2Int> BlockedCells => _blockedCells;

        public bool IsInside(Vector2Int cell)
        {
            return cell.x >= 0 && cell.y >= 0 && cell.x < Width && cell.y < Height;
        }

        public bool IsBlocked(Vector2Int cell)
        {
            return _blockedCells.Contains(cell);
        }

        public bool TryGetObject(string objectId, out GridObject gridObject)
        {
            return _objects.TryGetValue(objectId, out gridObject);
        }

        public bool TryGetObjectAt(Vector2Int cell, out GridObject gridObject)
        {
            gridObject = null;
            return _occupancy.TryGetValue(cell, out string objectId) && _objects.TryGetValue(objectId, out gridObject);
        }

        public GridCellValidation ValidateCells(IReadOnlyList<Vector2Int> cells, string ignoreObjectId = null)
        {
            if (cells == null || cells.Count == 0)
            {
                return new GridCellValidation(false, GridFailReason.InvalidCell, new List<Vector2Int>());
            }

            HashSet<Vector2Int> seen = new HashSet<Vector2Int>();
            List<Vector2Int> invalid = new List<Vector2Int>();
            GridFailReason reason = GridFailReason.None;

            for (int i = 0; i < cells.Count; i++)
            {
                Vector2Int cell = cells[i];
                if (!seen.Add(cell))
                {
                    invalid.Add(cell);
                    reason = GridFailReason.InvalidCell;
                    continue;
                }

                if (!IsInside(cell))
                {
                    invalid.Add(cell);
                    reason = reason == GridFailReason.None ? GridFailReason.OutOfBounds : reason;
                    continue;
                }

                if (IsBlocked(cell))
                {
                    invalid.Add(cell);
                    reason = reason == GridFailReason.None ? GridFailReason.BlockedCell : reason;
                    continue;
                }

                if (_occupancy.TryGetValue(cell, out string ownerId) && ownerId != ignoreObjectId)
                {
                    invalid.Add(cell);
                    reason = reason == GridFailReason.None ? GridFailReason.OccupiedCell : reason;
                }
            }

            return new GridCellValidation(invalid.Count == 0, reason, invalid);
        }

        public bool CanOccupy(IReadOnlyList<Vector2Int> cells, string ignoreObjectId = null)
        {
            return ValidateCells(cells, ignoreObjectId).IsValid;
        }

        public GridMoveResult AddObject(GridObject gridObject, IGridShapeProvider shapeProvider)
        {
            if (gridObject == null || string.IsNullOrWhiteSpace(gridObject.Id) || _objects.ContainsKey(gridObject.Id))
            {
                return GridMoveResult.Failed(GridFailReason.InvalidObject);
            }

            IReadOnlyList<Vector2Int> cells = gridObject.GetOccupiedCells(shapeProvider);
            if (gridObject.Kind == GridObjectKind.Snake && !GridSnakeUtility.IsValidSnakeCells(cells))
            {
                return GridMoveResult.Failed(GridFailReason.InvalidCell);
            }

            GridCellValidation validation = ValidateCells(cells);
            if (!validation.IsValid)
            {
                return GridMoveResult.Failed(validation.Reason);
            }

            _objects.Add(gridObject.Id, gridObject);
            SetOccupancy(gridObject.Id, cells);
            return GridMoveResult.Succeeded(gridObject.Pivot, cells);
        }

        public GridMoveResult MoveObject(string objectId, Vector2Int pivot, int rotation, IGridShapeProvider shapeProvider)
        {
            if (!_objects.TryGetValue(objectId, out GridObject gridObject))
            {
                return GridMoveResult.Failed(GridFailReason.InvalidObject);
            }

            if (gridObject.Kind == GridObjectKind.Snake)
            {
                return GridMoveResult.Failed(GridFailReason.InvalidObject);
            }

            if (!shapeProvider.TryGetShape(gridObject.ShapeId, out GridShape shape))
            {
                return GridMoveResult.Failed(GridFailReason.InvalidShape);
            }

            IReadOnlyList<Vector2Int> cells = shape.GetWorldCells(pivot, rotation);
            GridCellValidation validation = ValidateCells(cells, objectId);
            if (!GridSnakeUtility.IsValidSnakeCells(cells))
            {
                return GridMoveResult.Failed(GridFailReason.InvalidCell);
            }

            if (!validation.IsValid)
            {
                return GridMoveResult.Failed(validation.Reason);
            }

            ClearOccupancy(objectId);
            gridObject.SetBlockTransform(pivot, rotation);
            SetOccupancy(objectId, cells);
            return GridMoveResult.Succeeded(pivot, cells);
        }

        public GridMoveResult MoveSnake(string objectId, IReadOnlyList<Vector2Int> cells)
        {
            if (!_objects.TryGetValue(objectId, out GridObject gridObject) || gridObject.Kind != GridObjectKind.Snake)
            {
                return GridMoveResult.Failed(GridFailReason.InvalidObject);
            }

            GridCellValidation validation = ValidateCells(cells, objectId);
            if (!validation.IsValid)
            {
                return GridMoveResult.Failed(validation.Reason);
            }

            ClearOccupancy(objectId);
            gridObject.SetSnakeCells(cells);
            SetOccupancy(objectId, cells);
            return GridMoveResult.Succeeded(gridObject.Pivot, cells);
        }

        public bool RemoveObject(string objectId)
        {
            if (!_objects.Remove(objectId))
            {
                return false;
            }

            ClearOccupancy(objectId);
            return true;
        }

        private void SetOccupancy(string objectId, IReadOnlyList<Vector2Int> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                _occupancy[cells[i]] = objectId;
            }
        }

        private void ClearOccupancy(string objectId)
        {
            List<Vector2Int> toRemove = new List<Vector2Int>();
            foreach (KeyValuePair<Vector2Int, string> pair in _occupancy)
            {
                if (pair.Value == objectId)
                {
                    toRemove.Add(pair.Key);
                }
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                _occupancy.Remove(toRemove[i]);
            }
        }
    }
}
