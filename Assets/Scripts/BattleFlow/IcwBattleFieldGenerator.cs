using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using IcwField;



namespace IcwBattle
{
    
    class IcwBattleFieldGenerator
    {
        public enum MapTiles { Mud, Water, Rock, Gravel, Grass, Road, Empty };
        /*Dictionary<string, Color> cl = new Dictionary<string, Color> { 
               { "Mud", new Color(0.4f, 0.4f, 0.2f) },
               { "Water", new Color(0, 0.0f, 0.3f) },
               { "Rock", new Color(0.8f, 0.6f, 0.2f) },
               { "Gravel", new Color(0.8f, 0.7f, 0.4f) },
               { "Grass", new Color(0.0f, 0.5f, 0.2f) },
               { "Road", new Color(0.5f, 0.5f, 0.5f) },
           };*/
        public void CreateMap(IField field, Tilemap MainTileMap, List<IcwFieldObjectType> objtypes)
        {
            // здесь генерим карту 
            // далее будут параметры карты - болотистая, скалистая, ровная и т.п.
            // пока пустой - карту сделал руками для тестов
            // тащим содержимое тайлов создаем объекты
            Vector2Int fieldSize = field.Size;
            for (int x = 0; x < fieldSize.x; x++)
            {
                for (int y = 0; y < fieldSize.y; y++)
                {
                    field.battlefield[x, y] = new List<IFieldObject>();
                    IFieldObject newobj = new IcwBaseFieldObject();
                    TileBase currtile = MainTileMap.GetTile(new Vector3Int(x, y, 0));
                    IcwFieldObjectType currtiletype;
                    if (currtile !=null)
                        currtiletype = objtypes.Find(o => o.TileSprite.Find(ts => ts == currtile));
                    else
                        currtiletype = objtypes.Find(o => o.FieldObjectTypeName == "Empty");
                    newobj.ObjectType = currtiletype;
                    field.AddObject(newobj, new Vector2Int(x, y));
                }
            }

        }
        
        public Texture2D CreateByPerlin(IField field, Tilemap MainTileMap, List<IcwFieldObjectType> objtypes)
        {
            Vector2Int fieldSize = field.Size;
            //int size = 256;
            Vector2 randomVector = Random.insideUnitCircle * Random.Range(10,100);
            Vector2Int startpoint = Vector2Int.RoundToInt(randomVector);
            float[ , ] map = new float[fieldSize.x, fieldSize.y];
            float scale = 3.5f;

            float minvalue = 1;
            float maxvalue = 0;
            for (int x = 0; x < fieldSize.x; x++)
            {
                for (int y = 0; y < fieldSize.y; y++)
                {
                    float xCoord = startpoint.x + (float)x / fieldSize.x * scale;
                    float yCoord = startpoint.y + (float)y / fieldSize.y * scale;
                    float sample = Mathf.PerlinNoise(xCoord, yCoord);
                    map[x, y] = sample;
                    if (sample < minvalue) minvalue = sample;
                    if (sample > maxvalue) maxvalue = sample;
                }
            }

            Color[] colors = new Color[fieldSize.x * fieldSize.y];
            Texture2D tex = new Texture2D(fieldSize.x, fieldSize.y);
            for (int x = 0; x < fieldSize.x; x++)
            {
                for (int y = 0; y < fieldSize.y; y++)
                {
                    field.battlefield[x, y] = new List<IFieldObject>();
                    float resSample = 0;
                    resSample = (map[x , y] - minvalue) * (maxvalue - minvalue);

                    colors[y * fieldSize.x + x] = new Color(resSample, resSample, resSample);
                    IcwFieldObjectType col;
                    switch (resSample)
                    {
                        case < 0.1f: col = objtypes.Find(o => o.FieldObjectTypeName == "Wall"); break; // MapTiles.Rock; 
                        case < 0.2f: col = objtypes.Find(o => o.FieldObjectTypeName == "Gravel"); break;  //MapTiles.Gravel; break;
                        case < 0.3f: col = objtypes.Find(o => o.FieldObjectTypeName == "Grass"); break; // MapTiles.Grass; break;
                        case < 0.7f: col = objtypes.Find(o => o.FieldObjectTypeName == "Empty"); break; //MapTiles.Road; break;
                        case < 0.8f: col = objtypes.Find(o => o.FieldObjectTypeName == "Grass"); break; // MapTiles.Grass; break;
                        case < 0.9f: col = objtypes.Find(o => o.FieldObjectTypeName == "Gravel"); break; //MapTiles.Gravel; break;
                        case >= 0.9f: col = objtypes.Find(o => o.FieldObjectTypeName == "Wall"); break; // MapTiles.Rock; break;
                        default: col = objtypes.Find(o => o.FieldObjectTypeName == "Empty"); break; //MapTiles.Empty; break;
                    }
                    TileBase tile = null;
                    if (col.TileSprite.Count > 0)
                        tile = col.TileSprite[Random.Range(0, col.TileSprite.Count)];
                    MainTileMap.SetTile(new Vector3Int(x, y, 0), tile);
                    IFieldObject newobj = new IcwBaseFieldObject();
                    newobj.ObjectType = col;
                    field.AddObject(newobj, new Vector2Int(x, y));
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }
    }
}
