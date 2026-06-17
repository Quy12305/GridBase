namespace GridBase
{
    public class GridPlaceableDefinition
    {
        public GridPlaceableDefinition(string id, string shapeId, int rotation, GridMoveAxis moveAxis)
        {
            Id = id;
            ShapeId = shapeId;
            Rotation = GridShapeRotation.Normalize(rotation);
            MoveAxis = moveAxis;
        }

        public string Id { get; }
        public string ShapeId { get; }
        public int Rotation { get; }
        public GridMoveAxis MoveAxis { get; }
    }
}
