using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using IcwField;



namespace IcwBattle
{
    class IcwBattleFieldGenerator
    {

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
    }
}
