using System.Collections.Generic;
using UnityEngine;

namespace GridBase
{
    public static class GridLevelLoader
    {
        public static GridLevelData FromJson(string json)
        {
            return string.IsNullOrWhiteSpace(json) ? null : JsonUtility.FromJson<GridLevelData>(json);
        }

        public static GridLevelData FromResources(string resourcePath)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);
            return textAsset == null ? null : FromJson(textAsset.text);
        }

        public static GridObject ToRuntimeObject(GridObjectData data)
        {
            GridObjectKind kind = GridEnumParser.ParseOrDefault(data.type, GridObjectKind.Block);
            GridMoveAxis axis = GridEnumParser.ParseOrDefault(data.moveAxis, GridMoveAxis.Free);
            Vector2Int pivot = data.pivot == null ? Vector2Int.zero : data.pivot.ToVector2Int();

            GridObject gridObject = new GridObject(data.id, kind, data.shapeId, pivot, data.rotation, axis);
            if (kind == GridObjectKind.Snake)
            {
                gridObject.SetSnakeCells(ToVector2IntList(data.cells));
            }

            return gridObject;
        }

        public static IReadOnlyList<Vector2Int> ToVector2IntList(IReadOnlyList<GridCoordData> cells)
        {
            List<Vector2Int> result = new List<Vector2Int>();
            if (cells == null)
            {
                return result;
            }

            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i] != null)
                {
                    result.Add(cells[i].ToVector2Int());
                }
            }

            return result;
        }
    }
}
