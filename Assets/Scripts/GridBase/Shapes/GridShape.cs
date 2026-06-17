using System.Collections.Generic;
using UnityEngine;

namespace GridBase
{
    public class GridShape
    {
        private readonly List<Vector2Int> _localCells;

        public GridShape(string id, IEnumerable<Vector2Int> localCells)
        {
            Id = id;
            _localCells = new List<Vector2Int>(localCells);
        }

        public string Id { get; }
        public IReadOnlyList<Vector2Int> LocalCells => _localCells;

        public IReadOnlyList<Vector2Int> GetWorldCells(Vector2Int pivot, int rotation)
        {
            List<Vector2Int> cells = new List<Vector2Int>(_localCells.Count);
            int normalized = GridShapeRotation.Normalize(rotation);

            for (int i = 0; i < _localCells.Count; i++)
            {
                cells.Add(pivot + GridShapeRotation.Rotate(_localCells[i], normalized));
            }

            return cells;
        }
    }
}
