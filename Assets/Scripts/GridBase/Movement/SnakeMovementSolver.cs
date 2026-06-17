using System.Collections.Generic;
using UnityEngine;

namespace GridBase
{
    public class SnakeMovementSolver
    {
        private readonly GridMap _map;

        public SnakeMovementSolver(GridMap map)
        {
            _map = map;
        }

        public GridMoveResult SolveSnakeMove(string objectId, GridDragHandle handle, Vector2Int targetCell)
        {
            if (!_map.TryGetObject(objectId, out GridObject gridObject) || gridObject.Kind != GridObjectKind.Snake)
            {
                return GridMoveResult.Failed(GridFailReason.InvalidObject);
            }

            if (handle != GridDragHandle.Head && handle != GridDragHandle.Tail)
            {
                return GridMoveResult.Failed(GridFailReason.InvalidDragHandle);
            }

            List<Vector2Int> current = new List<Vector2Int>(gridObject.SnakeCells);
            if (current.Count == 0)
            {
                return GridMoveResult.Failed(GridFailReason.InvalidObject);
            }

            Vector2Int activeCell = handle == GridDragHandle.Head ? current[0] : current[current.Count - 1];
            Vector2Int delta = targetCell - activeCell;
            if (delta == Vector2Int.zero)
            {
                return GridMoveResult.Succeeded(activeCell, current);
            }

            IReadOnlyList<Vector2Int> steps = GridPathUtility.BuildAxisStepPath(delta);

            for (int i = 0; i < steps.Count; i++)
            {
                Vector2Int nextCell = activeCell + steps[i];
                if (!TryStepSnake(objectId, current, handle, nextCell, out List<Vector2Int> next))
                {
                    break;
                }

                current = next;
                activeCell = nextCell;
            }

            IReadOnlyList<Vector2Int> original = gridObject.SnakeCells;
            bool changed = current.Count == original.Count && current.Count > 0 && current[0] != original[0];
            if (!changed && handle == GridDragHandle.Tail && current.Count > 0)
            {
                changed = current[current.Count - 1] != original[original.Count - 1];
            }

            return changed
                ? GridMoveResult.Succeeded(handle == GridDragHandle.Head ? current[0] : current[current.Count - 1], current)
                : GridMoveResult.Preview(false, GridFailReason.NoValidMove, activeCell, current);
        }

        private bool TryStepSnake(
            string objectId,
            IReadOnlyList<Vector2Int> current,
            GridDragHandle handle,
            Vector2Int nextCell,
            out List<Vector2Int> next)
        {
            next = null;
            Vector2Int activeCell = handle == GridDragHandle.Head ? current[0] : current[current.Count - 1];
            if (!GridPathUtility.AreAdjacent(activeCell, nextCell))
            {
                return false;
            }

            List<Vector2Int> candidate = new List<Vector2Int>(current.Count);
            if (handle == GridDragHandle.Head)
            {
                candidate.Add(nextCell);
                for (int i = 0; i < current.Count - 1; i++)
                {
                    candidate.Add(current[i]);
                }
            }
            else
            {
                for (int i = 1; i < current.Count; i++)
                {
                    candidate.Add(current[i]);
                }

                candidate.Add(nextCell);
            }

            GridCellValidation validation = _map.ValidateCells(candidate, objectId);
            if (!validation.IsValid)
            {
                return false;
            }

            next = candidate;
            return true;
        }
    }

}
