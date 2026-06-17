using System;
using System.Collections.Generic;
using UnityEngine;

namespace GridBase
{
    [Serializable]
    public sealed class GridLevelData
    {
        public string levelId;
        public GridMapData map;
        public List<GridObjectData> objects = new List<GridObjectData>();
        public List<GridPlaceableItemData> placeableItems = new List<GridPlaceableItemData>();
    }

    [Serializable]
    public sealed class GridMapData
    {
        public int width;
        public int height;
        public List<GridCoordData> blockedCells = new List<GridCoordData>();
    }

    [Serializable]
    public sealed class GridObjectData
    {
        public string id;
        public string type;
        public string shapeId;
        public GridCoordData pivot;
        public int rotation;
        public string moveAxis;
        public List<GridCoordData> cells = new List<GridCoordData>();
    }

    [Serializable]
    public sealed class GridPlaceableItemData
    {
        public string id;
        public string shapeId;
        public int rotation;
        public string moveAxis;
    }

    [Serializable]
    public sealed class GridCoordData
    {
        public int x;
        public int y;

        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(x, y);
        }
    }
}
