using System.Collections.Generic;
using UnityEngine;

namespace GridBase
{
    public class GridObject
    {
        private readonly List<Vector2Int> _snakeCells = new List<Vector2Int>();

        public GridObject(
            string id,
            GridObjectKind kind,
            string shapeId,
            Vector2Int pivot,
            int rotation,
            GridMoveAxis moveAxis)
        {
            Id = id;
            Kind = kind;
            ShapeId = shapeId;
            Pivot = pivot;
            Rotation = GridShapeRotation.Normalize(rotation);
            MoveAxis = moveAxis;
        }

        public string Id { get; }
        public GridObjectKind Kind { get; }
        public string ShapeId { get; private set; }
        public Vector2Int Pivot { get; private set; }
        public int Rotation { get; private set; }
        public GridMoveAxis MoveAxis { get; private set; }
        public IReadOnlyList<Vector2Int> SnakeCells => _snakeCells;

        public virtual IReadOnlyList<Vector2Int> GetOccupiedCells(IGridShapeProvider shapeProvider)
        {
            if (Kind == GridObjectKind.Snake)
            {
                return new List<Vector2Int>(_snakeCells);
            }

            if (!shapeProvider.TryGetShape(ShapeId, out GridShape shape))
            {
                return new List<Vector2Int>();
            }

            return shape.GetWorldCells(Pivot, Rotation);
        }

        public virtual bool ContainsCell(Vector2Int cell, IGridShapeProvider shapeProvider)
        {
            IReadOnlyList<Vector2Int> cells = GetOccupiedCells(shapeProvider);
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i] == cell)
                {
                    return true;
                }
            }

            return false;
        }

        public virtual GridDragHandle GetSnakeHandle(Vector2Int cell)
        {
            if (Kind != GridObjectKind.Snake || _snakeCells.Count == 0)
            {
                return GridDragHandle.None;
            }

            if (_snakeCells[0] == cell)
            {
                return GridDragHandle.Head;
            }

            if (_snakeCells[_snakeCells.Count - 1] == cell)
            {
                return GridDragHandle.Tail;
            }

            for (int i = 1; i < _snakeCells.Count - 1; i++)
            {
                if (_snakeCells[i] == cell)
                {
                    return GridDragHandle.Body;
                }
            }

            return GridDragHandle.None;
        }

        public virtual void SetBlockTransform(Vector2Int pivot, int rotation)
        {
            Pivot = pivot;
            Rotation = GridShapeRotation.Normalize(rotation);
        }

        public virtual void SetMoveAxis(GridMoveAxis moveAxis)
        {
            MoveAxis = moveAxis;
        }

        public virtual void SetShape(string shapeId)
        {
            ShapeId = shapeId;
        }

        public virtual void SetSnakeCells(IReadOnlyList<Vector2Int> cells)
        {
            _snakeCells.Clear();
            if (cells == null)
            {
                return;
            }

            _snakeCells.AddRange(cells);
            if (_snakeCells.Count > 0)
            {
                Pivot = _snakeCells[0];
            }
        }

        public virtual GridObject Clone()
        {
            GridObject clone = new GridObject(Id, Kind, ShapeId, Pivot, Rotation, MoveAxis);
            clone.SetSnakeCells(_snakeCells);
            return clone;
        }
    }
}
