using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

namespace IcwField
{
    public class IcwField : MonoBehaviour, IField
    {
        public int SizeX { get; set; } = 14;
        public int SizeY { get; set; } = 11;
        Vector2Int IField.Size => new Vector2Int(SizeX, SizeY);
        Vector3 IField.GetWorldCoord(Vector2Int tilecoord) => MainTileMap.GetCellCenterWorld((Vector3Int)(tilecoord));
        Vector2Int IField.GetTileCoord(Vector3 worldPosition) => (Vector2Int)MainTileMap.WorldToCell(worldPosition);
        bool IField.IsValidTileCoord(Vector2Int pos) => pos.x >= 0 && pos.y >= 0 && pos.x < SizeX && pos.y < SizeY;
        public List<IFieldObject>[,] battlefield { get; set; }
        // Присоединенные префабы
        public GameObject AreaTileMapObject;
        public Tilemap MainTileMap;
        public Tilemap AreaTileMap;
        public TileBase[] TileBorder;


        private void Awake()
        {
            AreaTileMapObject.TryGetComponent<Tilemap>(out AreaTileMap);
            if (IcwAtomFunc.IsNull(AreaTileMap, this.name))
            {
                Destroy(this.gameObject);
                return;
            }
            battlefield = new List<IFieldObject>[SizeX, SizeY];
        }

        void IField.RemoveObject(IFieldObject obj)
        {
            for (int x = 0; x < SizeX; x++)
                for (int y = 0; y < SizeY; y++)
                    if ((this as IField).battlefield[x, y].Contains(obj))
                        (this as IField).battlefield[x, y].Remove(obj);
        }

        void IField.AddObject(IFieldObject obj, Vector2Int pos)
        {
            if (!(this as IField).IsValidTileCoord(pos)) return;
            if (!(this as IField).battlefield[pos.x, pos.y].Contains(obj))
                (this as IField).battlefield[pos.x, pos.y].Add(obj);
            (obj as IFieldObject).FieldPosition = pos;
            (obj as IFieldObject).Field = this;
        }

        void IField.MoveObject(IFieldObject obj, Vector2Int newpos)
        {
            if (!(this as IField).IsValidTileCoord(newpos)) return;
            (this as IField).RemoveObject(obj);
            (this as IField).AddObject(obj, newpos);
        }

        bool IField.IsObjectInTile(IFieldObject obj, Vector2Int pos)
        {
            if (!(this as IField).IsValidTileCoord(pos)) return false;
            return battlefield[pos.x, pos.y].Contains(obj);
        }
        List<IFieldObject> IField.GetObjectsInTile(Vector2Int pos)
        {
            if (!(this as IField).IsValidTileCoord(pos)) return new List<IFieldObject>();
            return (this as IField).battlefield[pos.x, pos.y];
        }

        Vector2Int[] IField.GetTileNeigbours(Vector2Int tileCoord)
        {
            Vector2Int[] checktemplateeven = { Vector2Int.up + tileCoord, Vector2Int.one + tileCoord, Vector2Int.right + tileCoord, new Vector2Int(1, -1) + tileCoord, Vector2Int.down + tileCoord, Vector2Int.left + tileCoord };
            Vector2Int[] checktemplateodd = { new Vector2Int(-1, 1) + tileCoord, Vector2Int.up + tileCoord, Vector2Int.right + tileCoord, Vector2Int.down + tileCoord, new Vector2Int(-1, -1) + tileCoord, Vector2Int.left + tileCoord };
            return tileCoord.y % 2 == 1 ? checktemplateeven : checktemplateodd;
        }

        int IField.Distance(Vector2Int tile1, Vector2Int tile2) =>
            IcwHexTile.Distance(IcwHexTile.OrthoToCube(tile1), IcwHexTile.OrthoToCube(tile2));

        List<Vector2Int> IField.GetAreaInRange(Vector2Int pos, int range)
        {
            List<Vector2Int> result = new List<Vector2Int>();
            Vector3Int tile = IcwHexTile.OrthoToCube(pos);
            for (int q = -range; q <= range; q++)
                for (int r = Mathf.Max(-range, -q - range); r <= Mathf.Min( range, -q + range); r++)
                {
                    Vector2Int currtile = IcwHexTile.CubeToOrtho(new Vector3Int(q, r, -q -r) + tile);
                    if ((this as IField).IsValidTileCoord(currtile))
                        result.Add(currtile);
                }
            return result;
        }

        
        public List<Vector2Int> GetTilesOnLine(Vector2Int startTile, Vector2Int targetTile)
        {
            List<Vector2Int> result = new();
            Vector3Int startCube = IcwHexTile.OrthoToCube(startTile);
            Vector3Int targetCube = IcwHexTile.OrthoToCube(targetTile);
            int dist = IcwHexTile.Distance(startCube, targetCube) ;

            for (int i = 0; i <= dist; i++)
            {
                Vector3 currPoint = Vector3.Lerp(startCube, targetCube, (float)i / dist);
                Vector2Int resTile = IcwHexTile.RoundCubeToOrhto(currPoint);
                result.Add(resTile);
            }
            return result;
        }

        bool IField.IsTileVisibleFrom(Vector2Int startTile, Vector2Int targetTile, out List<Vector2Int> visibletiles)
        {
            visibletiles = new();
            Vector3Int startCube = IcwHexTile.OrthoToCube(startTile);
            Vector3Int targetCube = IcwHexTile.OrthoToCube(targetTile);
            int dist = IcwHexTile.Distance(startCube, targetCube);

            for (int i = 0; i <= dist; i++)
            {
                Vector3 currPoint = Vector3.Lerp(startCube, targetCube, (float)i / dist);
                Vector2Int resTile = IcwHexTile.RoundCubeToOrhto(currPoint);
                //if (!battlefield[resTile.x, resTile.y].Exists(o => o.ObjectType == IFieldObject.ObjType.Wall || o.ObjectType == IFieldObject.ObjType.Unit))
                if (!battlefield[resTile.x, resTile.y].Exists(o => o.ObjectType.IsViewObstacle))
                    visibletiles.Add(resTile);
                else
                    return false;
            }
            return true;
        }
        Vector2Int? IField.GetFirstObstacleTile(Vector2Int startTile, Vector2Int targetTile, bool ObstacleMove, bool ObstacleVisible)
        {
            Vector3Int startCube = IcwHexTile.OrthoToCube(startTile);
            Vector3Int targetCube = IcwHexTile.OrthoToCube(targetTile);
            int dist = IcwHexTile.Distance(startCube, targetCube);

            for (int i = 1; i <= dist; i++)
            {
                Vector3 currPoint = Vector3.Lerp(startCube, targetCube, (float)i / dist);
                Vector2Int resTile = IcwHexTile.RoundCubeToOrhto(currPoint);
                if ((battlefield[resTile.x, resTile.y].Exists(o => o.ObjectType.IsViewObstacle && ObstacleVisible)) ||
                    (battlefield[resTile.x, resTile.y].Exists(o => o.ObjectType.IsMoveObstacle && ObstacleMove)))
                    return resTile;
            }
            return null;
        }

        List<Vector2Int> IField.GetVisibleTiles(Vector2Int pos, int range)
        {  
            List<Vector2Int> viewarea = (this as IField).GetAreaInRange(pos, range);
            List<Vector2Int> visibleTiles = new();
            List<Vector2Int> invisibleTiles = new();
            Vector3Int startCube = IcwHexTile.OrthoToCube(pos);
            Vector3 startWorldCoord = (this as IField).GetWorldCoord(pos);
            Vector3[] kEpsVectors = {new(Vector3.kEpsilon, Vector3.kEpsilon, Vector3.kEpsilon),
                                     new(-Vector3.kEpsilon, -Vector3.kEpsilon, -Vector3.kEpsilon) };
            int currrange = 1;
                while (viewarea.Count > 0 && currrange <= range)
                {
                    List<Vector2Int> ring = IcwHexTile.GetRing(pos, currrange);
                    foreach (Vector2Int v in ring)
                    {
                        Vector3Int targetCube = IcwHexTile.OrthoToCube(v);
                        Vector3 targetWorldCoord = (this as IField).GetWorldCoord(v);
                        int dist = IcwHexTile.Distance(startCube, targetCube);
                        bool IsThisVectorVisible = true;
                        for (int i = 1; i <= dist; i++)
                        {
                            Vector3 currPoint = Vector3.Lerp(startWorldCoord, targetWorldCoord, (float)i / dist);
                            List<Vector2Int> resTiles = new List<Vector2Int>();
                            Vector2Int resTile;
                            for (int epsindex = 0; epsindex<kEpsVectors.Length; epsindex++)
                            {
                                resTile = (this as IField).GetTileCoord(currPoint + kEpsVectors[epsindex]);
                                if (!resTiles.Contains(resTile) && (this as IField).IsValidTileCoord(resTile)) 
                                    resTiles.Add(resTile);
                            }
                            resTile = (this as IField).GetTileCoord(currPoint);
                            if (resTiles.Count == 0) 
                            { 
                                IsThisVectorVisible = false; 
                                continue; 
                            }
                        
                            if (viewarea.Contains(resTile))
                            {   // этот тайл еще не проверяли на видимость добавляем
                                if (IsThisVectorVisible)
                                    visibleTiles.Add(resTile);
                                else
                                    invisibleTiles.Add(resTile);
                                viewarea.Remove(resTile);
                            }
                            bool bLocalVisible = false;
                            foreach (Vector2Int rt in resTiles)
                            {
                                bLocalVisible |= !battlefield[rt.x, rt.y].Exists(o => o.ObjectType.IsViewObstacle);
                            }
                            IsThisVectorVisible &= bLocalVisible;
                        }
                    }
                    currrange++;
                }
            return visibleTiles;
        }

       
        

    }

}
