using System.Collections.Generic;
using UnityEngine;

namespace GridBase
{
    public class GridGameModel
    {
        private readonly Dictionary<string, GridPlaceableDefinition> _placeableDefinitions =
            new Dictionary<string, GridPlaceableDefinition>();

        private string _activePlaceableId;

        public GridGameModel()
            : this(GridShapeLibrary.CreateDefault())
        {
        }

        public GridGameModel(GridShapeLibrary shapeLibrary)
        {
            ShapeLibrary = shapeLibrary;
        }

        public GridShapeLibrary ShapeLibrary { get; }
        public GridMap Map { get; private set; }
        public GridDragState ActiveDrag { get; private set; }
        public IReadOnlyDictionary<string, GridPlaceableDefinition> PlaceableDefinitions => _placeableDefinitions;

        public virtual bool LoadLevel(GridLevelData levelData, out GridFailReason failReason)
        {
            failReason = GridFailReason.None;
            if (levelData == null || levelData.map == null || levelData.map.width <= 0 || levelData.map.height <= 0)
            {
                failReason = GridFailReason.InvalidMap;
                return false;
            }

            Map = new GridMap(
                levelData.map.width,
                levelData.map.height,
                GridLevelLoader.ToVector2IntList(levelData.map.blockedCells));

            _placeableDefinitions.Clear();
            ActiveDrag = null;
            _activePlaceableId = null;

            if (levelData.objects != null)
            {
                for (int i = 0; i < levelData.objects.Count; i++)
                {
                    GridObject gridObject = GridLevelLoader.ToRuntimeObject(levelData.objects[i]);
                    GridMoveResult addResult = Map.AddObject(gridObject, ShapeLibrary);
                    if (!addResult.Success)
                    {
                        failReason = addResult.Reason;
                        return false;
                    }
                }
            }

            if (levelData.placeableItems != null)
            {
                for (int i = 0; i < levelData.placeableItems.Count; i++)
                {
                    GridPlaceableItemData item = levelData.placeableItems[i];
                    GridMoveAxis axis = GridEnumParser.ParseOrDefault(item.moveAxis, GridMoveAxis.Free);
                    _placeableDefinitions[item.id] =
                        new GridPlaceableDefinition(item.id, item.shapeId, item.rotation, axis);
                }
            }

            return true;
        }

        public virtual bool LoadLevelFromResources(string resourcePath, out GridFailReason failReason)
        {
            return LoadLevel(GridLevelLoader.FromResources(resourcePath), out failReason);
        }

        public virtual GridMoveResult TryBeginDrag(Vector2Int touchedCell)
        {
            EnsureMap();
            ActiveDrag = null;

            if (!Map.TryGetObjectAt(touchedCell, out GridObject gridObject))
            {
                return GridMoveResult.Failed(GridFailReason.InvalidObject);
            }

            if (gridObject.Kind == GridObjectKind.Snake)
            {
                GridDragHandle handle = gridObject.GetSnakeHandle(touchedCell);
                if (handle != GridDragHandle.Head && handle != GridDragHandle.Tail)
                {
                    return GridMoveResult.Failed(GridFailReason.InvalidDragHandle);
                }

                ActiveDrag = new GridDragState(
                    gridObject.Id,
                    handle == GridDragHandle.Head ? GridDragMode.SnakeHead : GridDragMode.SnakeTail,
                    handle,
                    touchedCell);
            }
            else
            {
                ActiveDrag = new GridDragState(gridObject.Id, GridDragMode.Block, GridDragHandle.Body, touchedCell);
            }

            return GridMoveResult.Succeeded(gridObject.Pivot, gridObject.GetOccupiedCells(ShapeLibrary));
        }

        public virtual GridMoveResult PreviewActiveDrag(Vector2Int targetCell)
        {
            EnsureMap();
            if (ActiveDrag == null)
            {
                return GridMoveResult.Failed(GridFailReason.InvalidObject);
            }

            if (ActiveDrag.Mode == GridDragMode.Block)
            {
                return new GridMovementSolver(Map, ShapeLibrary)
                    .SolveBlockMove(ActiveDrag.ObjectId, ActiveDrag.StartPointerCell, targetCell);
            }

            return new SnakeMovementSolver(Map)
                .SolveSnakeMove(ActiveDrag.ObjectId, ActiveDrag.Handle, targetCell);
        }

        public virtual GridMoveResult ApplyActiveDrag(Vector2Int targetCell)
        {
            GridMoveResult result = PreviewActiveDrag(targetCell);
            if (!result.Success)
            {
                return result;
            }

            GridMoveResult commitResult = ApplyDragResult(result);
            if (commitResult.Success)
            {
                ActiveDrag.ResetStartPointer(targetCell);
            }

            return commitResult;
        }

        public virtual GridMoveResult CommitActiveDrag(Vector2Int targetCell)
        {
            GridMoveResult result = PreviewActiveDrag(targetCell);
            if (!result.Success)
            {
                ActiveDrag = null;
                return result;
            }

            GridMoveResult commitResult = ApplyDragResult(result);
            ActiveDrag = null;
            return commitResult;
        }

        public virtual void CancelActiveDrag()
        {
            ActiveDrag = null;
        }

        public virtual bool BeginPlacement(string placeableId)
        {
            _activePlaceableId = _placeableDefinitions.ContainsKey(placeableId) ? placeableId : null;
            return _activePlaceableId != null;
        }

        public virtual GridMoveResult PreviewPlacement(Vector2Int pivot)
        {
            EnsureMap();
            if (string.IsNullOrEmpty(_activePlaceableId) ||
                !_placeableDefinitions.TryGetValue(_activePlaceableId, out GridPlaceableDefinition definition))
            {
                return GridMoveResult.Failed(GridFailReason.InvalidObject);
            }

            return new PlacementSolver(Map, ShapeLibrary).Preview(definition.ShapeId, pivot, definition.Rotation);
        }

        public virtual GridMoveResult CommitPlacement(string newObjectId, Vector2Int pivot)
        {
            EnsureMap();
            if (string.IsNullOrEmpty(_activePlaceableId) ||
                !_placeableDefinitions.TryGetValue(_activePlaceableId, out GridPlaceableDefinition definition))
            {
                return GridMoveResult.Failed(GridFailReason.InvalidObject);
            }

            GridFailReason failure = new PlacementSolver(Map, ShapeLibrary)
                .GetPlacementFailure(definition.ShapeId, pivot, definition.Rotation);
            if (failure != GridFailReason.None)
            {
                return GridMoveResult.Failed(failure);
            }

            GridObject gridObject = new GridObject(
                newObjectId,
                GridObjectKind.Block,
                definition.ShapeId,
                pivot,
                definition.Rotation,
                definition.MoveAxis);

            GridMoveResult result = Map.AddObject(gridObject, ShapeLibrary);
            if (result.Success)
            {
                _activePlaceableId = null;
            }

            return result;
        }

        public virtual void CancelPlacement()
        {
            _activePlaceableId = null;
        }

        public virtual IReadOnlyList<Vector2Int> GetObjectCells(string objectId)
        {
            EnsureMap();
            return Map.TryGetObject(objectId, out GridObject gridObject)
                ? gridObject.GetOccupiedCells(ShapeLibrary)
                : new List<Vector2Int>();
        }

        private GridMoveResult ApplyDragResult(GridMoveResult result)
        {
            if (ActiveDrag.Mode == GridDragMode.Block)
            {
                if (!Map.TryGetObject(ActiveDrag.ObjectId, out GridObject gridObject))
                {
                    return GridMoveResult.Failed(GridFailReason.InvalidObject);
                }

                return Map.MoveObject(ActiveDrag.ObjectId, result.Pivot, gridObject.Rotation, ShapeLibrary);
            }

            return Map.MoveSnake(ActiveDrag.ObjectId, result.Cells);
        }

        private void EnsureMap()
        {
            if (Map == null)
            {
                throw new System.InvalidOperationException("GridGameModel has no loaded GridMap.");
            }
        }
    }
}
