using UnityEngine;

namespace GridBase
{
    public class GridInteractionService
    {
        private readonly GridGameModel _model;

        public GridInteractionService(GridGameModel model)
        {
            _model = model;
        }

        public GridMoveResult PointerDown(Vector2Int cell)
        {
            return _model.TryBeginDrag(cell);
        }

        public GridMoveResult PointerMovePreview(Vector2Int cell)
        {
            return _model.PreviewActiveDrag(cell);
        }

        public GridMoveResult PointerMoveApply(Vector2Int cell)
        {
            return _model.ApplyActiveDrag(cell);
        }

        public GridMoveResult PointerUp(Vector2Int cell)
        {
            return _model.CommitActiveDrag(cell);
        }

        public void CancelPointer()
        {
            _model.CancelActiveDrag();
        }

        public bool SelectPlaceable(string placeableId)
        {
            return _model.BeginPlacement(placeableId);
        }

        public GridMoveResult MovePlaceablePreview(Vector2Int pivot)
        {
            return _model.PreviewPlacement(pivot);
        }

        public GridMoveResult PlaceSelected(string newObjectId, Vector2Int pivot)
        {
            return _model.CommitPlacement(newObjectId, pivot);
        }

        public void CancelPlaceable()
        {
            _model.CancelPlacement();
        }
    }
}
