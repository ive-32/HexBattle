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
        private Vector2Int LeftDownAngle = new Vector2Int(10, 3);
        Vector2Int IField.GetSize => new Vector2Int(SizeX, SizeY);
        Vector3 IField.GetWorldCoord(Vector2Int tilecoord) => MainTileMap.GetCellCenterWorld((Vector3Int)(tilecoord + LeftDownAngle));
        Vector2Int IField.GetTileCoord(Vector3 worldPosition) => (Vector2Int)MainTileMap.WorldToCell(worldPosition) - LeftDownAngle;
        bool IField.IsValidTileCoord(Vector2Int pos) => pos.x >= 0 && pos.y >= 0 && pos.x < SizeX && pos.y < SizeY;
        public List<IFieldObject>[,] battlefield { get; set; }
        // Присоединенные префабы
        public GameObject MainTileMapObject;
        public GameObject AreaTileMapObject;
        private Tilemap MainTileMap;
        private Tilemap AreaTileMap;
        public GameObject TextPrefab; // для отладки
        public GameObject DebugLayer; // для отладки
        public TileBase[] TileBorder;
        List<GameObject> unitObjects = new();

        public void CreateMap()
        {
            // здесь генерим карту - если генерация сложная то отдельный класс MapGenerator
            // с интерфейсом который примет IBattleObject[,] battlefield { get; set; }
            // и параметры карты - болотистая, скалистая, ровная и т.п.
            // если простой то процедура
            // пока пустой - карту сделал руками для тестов
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    battlefield[x, y] = new List<IFieldObject>();
                    IFieldObject newobj = new IcwBaseFieldObject();
                    TileBase tb = MainTileMap.GetTile(new Vector3Int(x, y, 0) + (Vector3Int)LeftDownAngle);
                    if (tb == null)
                    {
                        (this as IField).AddObject(newobj, new Vector2Int(x, y));
                        continue;
                    }
                    TileBase currtile = MainTileMap.GetTile(new Vector3Int(x, y, 0) + (Vector3Int)LeftDownAngle);
                    string name = currtile.name;

                    if (name.Contains("Grass"))
                    {
                        
                        //(this as IField).AddObject(new IcwBaseFieldObject(), new Vector2Int(x, y));

                    }
                    if (name.Contains("Gravel"))
                        //(this as IField).AddObject(new IcwBaseFieldObject(IFieldObject.ObjType.Gravel), new Vector2Int(x, y));
                        ;
                    if (name.Contains("Wall"))
                        //(this as IField).AddObject(new IcwBaseFieldObject(IFieldObject.ObjType.Wall), new Vector2Int(x, y));
                        ;
                }
            }

        }

        private void Awake()
        {
            if (IcwAtomFunc.IsNull(MainTileMapObject, this.name))
            {
                Destroy(this.gameObject);
                return;
            }
            MainTileMapObject.TryGetComponent<Tilemap>(out MainTileMap);
            if (IcwAtomFunc.IsNull(MainTileMap, this.name))
            {
                Destroy(this.gameObject);
                return;
            }

            AreaTileMapObject.TryGetComponent<Tilemap>(out AreaTileMap);
            if (IcwAtomFunc.IsNull(AreaTileMap, this.name))
            {
                Destroy(this.gameObject);
                return;
            }
            battlefield = new List<IFieldObject>[SizeX, SizeY];
            //CreateMap();
            
        }

        void IField.RemoveObject(IFieldObject obj)
        {
            for (int x = 0; x < SizeX; x++)
                for (int y = 0; y < SizeY; y++)
                    if ((this as IField).battlefield[x, y].Contains(obj))
                        (this as IField).battlefield[x, y].Remove(obj);
            (obj as IFieldObject).FieldPosition = new Vector2Int(-1, -1);
        }

        void IField.AddObject(IFieldObject obj, Vector2Int pos)
        {
            if (!(this as IField).IsValidTileCoord(pos)) return;
            if (!(this as IField).battlefield[pos.x, pos.y].Contains(obj))
                (this as IField).battlefield[pos.x, pos.y].Add(obj);
            (obj as IFieldObject).FieldPosition = pos;
        }

        void IField.MoveObject(IFieldObject obj, Vector2Int newpos)
        {
            if (!(this as IField).IsValidTileCoord(newpos)) return;
            // если это не GameObject то и двигать нечего
            // пока двигаем только юниты! если будет иное то дописать логику в AddObject
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

        Vector2Int[] GetTileNeigbours(Vector2Int tileCoord)
        {
            Vector2Int[] checktemplateodd = { Vector2Int.up + tileCoord, Vector2Int.one + tileCoord, Vector2Int.right + tileCoord, new Vector2Int(1, -1) + tileCoord, Vector2Int.down + tileCoord, Vector2Int.left + tileCoord };
            Vector2Int[] checktemplateeven = { new Vector2Int(-1, 1) + tileCoord, Vector2Int.up + tileCoord, Vector2Int.right + tileCoord, Vector2Int.down + tileCoord, new Vector2Int(-1, -1) + tileCoord, Vector2Int.left + tileCoord };
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
        Vector2Int? IField.GetFirstObstacleTile(Vector2Int startTile, Vector2Int targetTile)
        {
            Vector3Int startCube = IcwHexTile.OrthoToCube(startTile);
            Vector3Int targetCube = IcwHexTile.OrthoToCube(targetTile);
            int dist = IcwHexTile.Distance(startCube, targetCube);

            for (int i = 1; i <= dist; i++)
            {
                Vector3 currPoint = Vector3.Lerp(startCube, targetCube, (float)i / dist);
                Vector2Int resTile = IcwHexTile.RoundCubeToOrhto(currPoint);
                if (battlefield[resTile.x, resTile.y].Exists(o => o.ObjectType.IsViewObstacle))
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

            while (viewarea.Count > 0 && range>0)
            {
                List<Vector2Int> ring = IcwHexTile.GetRing(pos, range);
                foreach (Vector2Int v in ring)
                {
                    Vector3Int targetCube = IcwHexTile.OrthoToCube(v);
                    int dist = IcwHexTile.Distance(startCube, targetCube);
                    bool IsThisVectorVisible = true;
                    for (int i = 1; i <= dist; i++)
                    {
                        Vector3 currPoint = Vector3.Lerp(startCube, targetCube, (float)i / dist);
                        Vector2Int resTile = IcwHexTile.RoundCubeToOrhto(currPoint);
                        if (!(this as IField).IsValidTileCoord(resTile)) 
                        { 
                            IsThisVectorVisible = false; 
                            continue; 
                        }
                        if (battlefield[resTile.x, resTile.y] == null)
                        {
                            Debug.LogError($"tile {resTile} has no Any object!");
                            continue;
                        }
                        if (battlefield[resTile.x, resTile.y].Count <= 0) continue;
                        if (battlefield[resTile.x, resTile.y].Count > 1)
                        {
                            bool isobs = battlefield[resTile.x, resTile.y][1].ObjectType.IsViewObstacle;
                            Debug.LogWarning($"tile {isobs} now checked");
                        }
                        IsThisVectorVisible &= !battlefield[resTile.x, resTile.y].Exists(o => o.ObjectType.IsViewObstacle);
                        if (!viewarea.Contains(resTile)) continue; // уже этот тайл проверяли на видимость, не добавляем 
                        
                        if (IsThisVectorVisible)
                            visibleTiles.Add(resTile);
                        else
                            invisibleTiles.Add(resTile);
                        viewarea.Remove(resTile);
                    }
                }
                range--;
            }
            return visibleTiles;
        }

        private void ShowCostForTile(Vector2Int pos, IcwStepWeigth weight)
        {
            if (weight == null) return;
            GameObject gm = Instantiate(TextPrefab, (this as IField).GetWorldCoord(new Vector2Int(pos.x, pos.y)), Quaternion.identity, DebugLayer.transform);
            gm.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI tmpro);
            if (IcwAtomFunc.IsNull(tmpro, this.name))
            {
                Destroy(gm);
                return;
            }
            tmpro.text = $"{weight.turn} t\n{weight.cost} tp";
        }

        void IField.ShowBorderTile(Vector2Int tile1, int? tiletype)
        {
            if (tiletype < 0) tiletype = 0;
            if (tiletype >= TileBorder.Length) tiletype = TileBorder.Length - 1;
            if (tiletype != null)
                AreaTileMap.SetTile(new Vector3Int(tile1.x, tile1.y, 0) + (Vector3Int)LeftDownAngle, TileBorder[(int)tiletype]);
            else
                AreaTileMap.SetTile(new Vector3Int(tile1.x, tile1.y, 0) + (Vector3Int)LeftDownAngle, null);
        }

        void IField.ShowCoordTile(bool show)
        {
            (this as IField).HideAllInfo();


            for (int x = 0; x < SizeX; x++)
                for (int y = 0; y < SizeY; y++)
                {
                    if (show)
                    {
                        AreaTileMap.SetTile(new Vector3Int(x, y, 0) + (Vector3Int)LeftDownAngle, TileBorder[3]);
                        GameObject gm = Instantiate(TextPrefab, (this as IField).GetWorldCoord(new Vector2Int(x, y)), Quaternion.identity, DebugLayer.transform);
                        gm.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI tmpro);
                        if (IcwAtomFunc.IsNull(tmpro, this.name))
                        {
                            Destroy(gm);
                            return;
                        }
                        tmpro.text = $"{x} - {y}";
                    }
                    else
                        AreaTileMap.SetTile(new Vector3Int(x, y, 0) + (Vector3Int)LeftDownAngle, null);
                }
        }

        void IField.ShowTurnArea(IcwStepWeigth[,] weights)//, IFieldObject fieldObject) это понадобится 
        {
            // отображаем на поле инормацию по юниту
            // сейчас показывем ходы и цену хода
            // далее будем подсвечивать тайлы куда можно пойти и куда можно стрелять
            // очищаем сетку с подсказками
            (this as IField).HideAllInfo();

            if (weights == null) return; // если не выбран ни один юнит или нет карты весов то возвращаемся
            for (int x = 0; x < SizeX; x++)
                for (int y = 0; y < SizeY; y++)
                {
                    if (weights[x, y] == null) continue;
                    if (weights[x, y].turn == IFieldObject.MaxStepCost || weights[x, y].cost == IFieldObject.MaxStepCost) continue;
                    if (IcwGlobalSettings.ShowStepCosts) ShowCostForTile(new Vector2Int(x, y), weights[x, y]);
                    if (weights[x, y].turn == 1) AreaTileMap.SetTile(new Vector3Int(x, y, 0) + (Vector3Int)LeftDownAngle, TileBorder[0]);
                    else AreaTileMap.SetTile(new Vector3Int(x, y, 0) + (Vector3Int)LeftDownAngle, null);
                }
        }
        void IField.ShowRoute(List<Vector2Int> route, IcwStepWeigth[,] weights)
        {
            (this as IField).ShowTurnArea(weights);
            if (route == null)
                return;
            foreach (Vector2Int v in route)
            {
                AreaTileMap.SetTile(new Vector3Int(v.x, v.y, 0) + (Vector3Int)LeftDownAngle, TileBorder[1]);
                ShowCostForTile(v, weights[v.x, v.y]);
            }
            //ShowCostForTile(route[0], obj.weights[route[0].x, route[0].y]);
        }

        void IField.HideAllInfo()
        {
            foreach (Transform child in DebugLayer.transform)
                GameObject.Destroy(child.gameObject);
            for (int x = 0; x < SizeX; x++)
                for (int y = 0; y < SizeY; y++)
                    AreaTileMap.SetTile(new Vector3Int(x, y, 0) + (Vector3Int)LeftDownAngle, null);
        }

    }

}
