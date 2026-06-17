using System.Collections.Generic;
using UnityEngine;

namespace GridBase
{
    public class GridShapeLibrary : IGridShapeProvider
    {
        public const string Cell1x1 = "Cell_1x1";
        public const string Rect1x2 = "Rect_1x2";
        public const string Rect2x2 = "Rect_2x2";
        public const string Rect1x3 = "Rect_1x3";
        public const string Rect2x3 = "Rect_2x3";
        public const string Rect3x3 = "Rect_3x3";
        public const string MissingCorner2x2 = "MissingCorner_2x2";
        public const string Plus3x3 = "Plus_3x3";

        private readonly Dictionary<string, GridShape> _shapes = new Dictionary<string, GridShape>();

        public static GridShapeLibrary CreateDefault()
        {
            GridShapeLibrary library = new GridShapeLibrary();
            library.Register(new GridShape(Cell1x1, new[] { new Vector2Int(0, 0) }));
            library.Register(Rect(Rect1x2, 1, 2));
            library.Register(Rect(Rect2x2, 2, 2));
            library.Register(Rect(Rect1x3, 1, 3));
            library.Register(Rect(Rect2x3, 2, 3));
            library.Register(Rect(Rect3x3, 3, 3, new Vector2Int(1, 1)));
            library.Register(new GridShape(MissingCorner2x2, new[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(0, 1)
            }));
            library.Register(new GridShape(Plus3x3, new[]
            {
                new Vector2Int(0, -1),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(0, 1)
            }));
            return library;
        }

        public void Register(GridShape shape)
        {
            _shapes[shape.Id] = shape;
        }

        public bool TryGetShape(string shapeId, out GridShape shape)
        {
            if (string.IsNullOrWhiteSpace(shapeId))
            {
                shape = null;
                return false;
            }

            return _shapes.TryGetValue(shapeId, out shape);
        }

        private static GridShape Rect(string id, int width, int height)
        {
            return Rect(id, width, height, Vector2Int.zero);
        }

        private static GridShape Rect(string id, int width, int height, Vector2Int pivotLocalCell)
        {
            List<Vector2Int> cells = new List<Vector2Int>(width * height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    cells.Add(new Vector2Int(x, y) - pivotLocalCell);
                }
            }

            return new GridShape(id, cells);
        }
    }
}
