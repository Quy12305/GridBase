using UnityEngine;

namespace GridBase
{
    public sealed class GridDragState
    {
        public GridDragState(string objectId, GridDragMode mode, GridDragHandle handle, Vector2Int startPointerCell)
        {
            ObjectId = objectId;
            Mode = mode;
            Handle = handle;
            StartPointerCell = startPointerCell;
        }

        public string ObjectId { get; }
        public GridDragMode Mode { get; }
        public GridDragHandle Handle { get; }
        public Vector2Int StartPointerCell { get; private set; }

        public void ResetStartPointer(Vector2Int cell)
        {
            StartPointerCell = cell;
        }
    }
}
