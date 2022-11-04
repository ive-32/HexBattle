using System.Collections.Generic;
using UnityEngine;
using IcwUnits;

namespace IcwField
{
    public interface IField
    {
        Vector2Int Size { get; }
        List<IFieldObject>[,] battlefield { get; set; }
        Vector3 GetWorldCoord(Vector2Int tilecoord);
        Vector2Int GetTileCoord(Vector3 worldPosition);
        bool IsValidTileCoord(Vector2Int pos);
        void RemoveObject(IFieldObject obj);
        void AddObject(IFieldObject obj, Vector2Int pos);
        void MoveObject(IFieldObject obj, Vector2Int newpos);
        bool IsObjectInTile(IFieldObject obj, Vector2Int pos);
        List<IFieldObject> GetObjectsInTile(Vector2Int pos);
        int Distance(Vector2Int tile1, Vector2Int tile2);
        Vector2Int[] GetTileNeigbours(Vector2Int tileCoord);
        List<Vector2Int> GetAreaInRange(Vector2Int pos, int range);
        public List<Vector2Int> GetTilesOnLine(Vector2Int startTile, Vector2Int targetTile);
        bool IsTileVisibleFrom(Vector2Int startTile, Vector2Int targetTile, out List<Vector2Int> visibletiles);
        Vector2Int? GetFirstObstacleTile(Vector2Int startTile, Vector2Int targetTile);
        List<Vector2Int> GetVisibleTiles(Vector2Int pos, int range);
        
        

    }
}
