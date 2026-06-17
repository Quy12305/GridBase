namespace GridBase
{
    public interface IGridShapeProvider
    {
        bool TryGetShape(string shapeId, out GridShape shape);
    }
}
