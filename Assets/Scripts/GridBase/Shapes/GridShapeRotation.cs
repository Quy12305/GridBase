using UnityEngine;

namespace GridBase
{
    public static class GridShapeRotation
    {
        public static int Normalize(int rotation)
        {
            int value = rotation % 4;
            return value < 0 ? value + 4 : value;
        }

        public static Vector2Int Rotate(Vector2Int cell, int rotation)
        {
            switch (Normalize(rotation))
            {
                case 1:
                    return new Vector2Int(cell.y, -cell.x);
                case 2:
                    return new Vector2Int(-cell.x, -cell.y);
                case 3:
                    return new Vector2Int(-cell.y, cell.x);
                default:
                    return cell;
            }
        }
    }
}
