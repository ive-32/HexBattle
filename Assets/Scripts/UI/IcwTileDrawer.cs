using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using IcwField;


namespace IcwUI
{
    public class IcwTileDrawer
    {
        IField Field { get; set; }
        public GameObject TextPrefab;
        GameObject FieldTextlayer;
        GameObject[,] TilesText;
        public Tilemap InfoTileMap;
        public List<TileBase> TilePrefabs = new List<TileBase>(); 

        public IcwTileDrawer(IField field, Tilemap tilemap)
        {
            Field = field;
            InfoTileMap = tilemap;
            InfoTileMap.transform.position = Field.GetWorldCoord(Vector2Int.zero);
            TilesText = new GameObject[Field.Size.x, Field.Size.y];
            FieldTextlayer = new GameObject("FieldTextLayer");
            //FieldTextlayer = GameObject.Instantiate(gm) as GameObject;
            //FieldTextlayer.name = "FieldTextLayer";
        }

        public void ShowTextForTile(Vector2Int pos, string atext)
        {
            if (TilesText[pos.x, pos.y] == null)
                TilesText[pos.x, pos.y] = Object.Instantiate(TextPrefab, Field.GetWorldCoord(new Vector2Int(pos.x, pos.y)), Quaternion.identity, FieldTextlayer.transform);
            TilesText[pos.x, pos.y].TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI tmpro);
            if (tmpro is TextMeshProUGUI)
                tmpro.text = atext;
            else
            {
                Object.Destroy(TilesText[pos.x, pos.y]);
                TilesText[pos.x, pos.y] = null;
            }
        }
        public void ShowBorderTile(Vector2Int tile1, TileBase tile)
        {
            InfoTileMap.SetTile(new Vector3Int(tile1.x, tile1.y, 0) , tile);
        }

        public void ShowBorderTile(Vector2Int tile1, int tiletype)
        {
            if (tiletype < 0) tiletype = 0;
            if (tiletype >= TilePrefabs.Count) tiletype = TilePrefabs.Count - 1;
                
            ShowBorderTile(tile1, TilePrefabs[tiletype]);
        }

        public void ShowTurnArea(IcwStepWeigth[,] weights)
        {
            // отображаем на поле инормацию по юниту
            // сейчас показывем ходы и цену хода
            // далее будем подсвечивать тайлы куда можно пойти и куда можно стрелять

            if (weights == null) return;
            Vector2Int fieldSize = Field.Size;
            for (int x = 0; x < fieldSize.x; x++)
                for (int y = 0; y < fieldSize.y; y++)
                {
                    if (weights[x, y] == null) continue;
                    if (weights[x, y].turn == IFieldObject.MaxStepCost || weights[x, y].cost == IFieldObject.MaxStepCost) continue;
                    if (IcwGlobalSettings.ShowStepCosts) ShowTextForTile(new Vector2Int(x, y), $"t {weights[x, y].turn} \n tp {weights[x, y].cost}");
                    if (weights[x, y].turn == 1) InfoTileMap.SetTile(new Vector3Int(x, y, 0), TilePrefabs[0]);
                    else InfoTileMap.SetTile(new Vector3Int(x, y, 0) , null);
                }
        }

        public void ShowRoute(List<Vector2Int> route, IcwStepWeigth[,] weights)
        {
            InfoTileMap.ClearAllTiles();
            ShowTurnArea(weights);
            if (route == null)
                return;
            foreach (Vector2Int v in route)
            {
                InfoTileMap.SetTile(new Vector3Int(v.x, v.y, 0), TilePrefabs[1]);
                ShowTextForTile(v, $"t {weights[v.x, v.y].turn} tp {weights[v.x, v.y].cost}");
            }
            //ShowCostForTile(route[0], obj.weights[route[0].x, route[0].y]);
        }
        public void ClearTiles()
        {
            foreach(Transform tm in FieldTextlayer.transform)
            {
                Object.Destroy(tm.gameObject);
            }
            InfoTileMap.ClearAllTiles();
        }

    }
}
