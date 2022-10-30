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
                    TileBase tb = MainTileMap.GetTile(new Vector3Int(x, y, 0) + (Vector3Int)LeftDownAngle);
                    if (tb == null)
                    {
                        (this as IField).AddObject(new IcwBaseFieldObject(), new Vector2Int(x, y));
                        continue;
                    }
                    string name = MainTileMap.GetTile(new Vector3Int(x, y, 0) + (Vector3Int)LeftDownAngle).name;
                    if (name.Contains("Grass"))
                        (this as IField).AddObject(new IcwBaseFieldObject(IFieldObject.ObjType.Grass), new Vector2Int(x, y));
                    if (name.Contains("Gravel"))
                        (this as IField).AddObject(new IcwBaseFieldObject(IFieldObject.ObjType.Gravel), new Vector2Int(x, y));
                    if (name.Contains("Wall"))
                        (this as IField).AddObject(new IcwBaseFieldObject(IFieldObject.ObjType.Wall), new Vector2Int(x, y));
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
            CreateMap();
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

        /*Vector2Int IField.GetObjectPosition(IFieldObject obj)
        {
            for (int x = 0; x < SizeX; x++)
                for (int y = 0; y < SizeY; y++)
                    if (battlefield[x, y].Contains(obj)) 
                        return new Vector2Int(x, y);
            // not fouded - return invalid coord
            return new Vector2Int(-1, -1);
        }*/

        bool IField.IsObjectInTile(IFieldObject obj, Vector2Int pos)
        {
            if (!(this as IField).IsValidTileCoord(pos)) return false;
            return battlefield[pos.x, pos.y].Contains(obj);
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

        void IField.ShowTurnArea(IcwStepWeigth[,] weights)//, IFieldObject fieldObject) это понадобится 
        {
            // отображаем на поле инормацию по юниту
            // сейчас показывем ходы и цену хода
            // далее будем подсвечивать тайлы куда можно пойти и куда можно стрелять
            // очищаем сетку с подсказками
            foreach (Transform child in DebugLayer.transform)
                GameObject.Destroy(child.gameObject);
            for (int x = 0; x < SizeX; x++)
                for (int y = 0; y < SizeY; y++)
                    AreaTileMap.SetTile(new Vector3Int(x, y, 0) + (Vector3Int)LeftDownAngle, null);

            if (weights == null) return; // если не выбран ни один юнит или нет карты весов то возвращаемся
            for (int x = 0; x < SizeX; x++)
                for (int y = 0; y < SizeY; y++)
                {
                    if (weights[x, y] == null) continue;
                    if (weights[x, y].turn == IFieldObject.MaxStepCost || weights[x, y].cost == IFieldObject.MaxStepCost) continue;
                    if (IcwGlobalSettings.ShowStepCosts) ShowCostForTile(new Vector2Int(x, y), weights[x, y]);
                    if (weights[x, y].turn == 1) AreaTileMap.SetTile(new Vector3Int(x, y, 0) + (Vector3Int)LeftDownAngle, TileBorder[4]);
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


    }

}
