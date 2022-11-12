using System.Collections.Generic;
using UnityEngine;
using IcwUnits;

namespace IcwField
{
    public class IcwWeightMapGenerator
    {
        IcwStepWeigth[,] weightmap;
        IField field;
        IUnit caller;
        List<Vector2Int> tilesToCheck = new List<Vector2Int>();
        
        /*Vector2Int GetNeigbourForHex(int index, bool ForEvenRow = false)
        {
            Vector2Int[] checktemplateodd = { Vector2Int.up, Vector2Int.one, Vector2Int.right, new Vector2Int(1, -1), Vector2Int.down, Vector2Int.left };
            Vector2Int[] checktemplateeven = { new Vector2Int(-1, 1), Vector2Int.up, Vector2Int.right, Vector2Int.down, new Vector2Int(-1, -1), Vector2Int.left };
            return ForEvenRow ? checktemplateeven[index] : checktemplateodd[index];
        }*/
        /*Vector2Int[] GetAllNeigboursForHex(bool ForEvenRow = false)
        {
            Vector2Int[] checktemplateeven = { Vector2Int.up, Vector2Int.one, Vector2Int.right, new Vector2Int(1, -1), Vector2Int.down, Vector2Int.left };
            Vector2Int[] checktemplateodd = { new Vector2Int(-1, 1), Vector2Int.up, Vector2Int.right, Vector2Int.down, new Vector2Int(-1, -1), Vector2Int.left };
            return ForEvenRow ? checktemplateeven : checktemplateodd;
        }*/

        private void CheckOneTile()
        {
            Vector2Int currTile = tilesToCheck[0];
            Vector2Int[] checktemplate = field.GetTileNeigbours(currTile);
            foreach (Vector2Int neigbourTile in checktemplate)
            {
                Vector2Int v = neigbourTile; // + currTile;
                if (!field.IsValidTileCoord(v)) continue;
                int turn = weightmap[currTile.x, currTile.y].turn;
                int newcost = caller.CostTile(field.battlefield[v.x, v.y]);

                int newTotalTurnCost = newcost + weightmap[currTile.x, currTile.y].cost;
                if (newTotalTurnCost > caller.CurrentStats.TurnPoints)
                {   // на предыдущем тайле закончился ход 
                    turn++;
                    newTotalTurnCost = newcost;
                }
                if (newcost >= IFieldObject.MaxStepCost)
                {   // непроходимый тайл, ставим все в максимум
                    newTotalTurnCost = IFieldObject.MaxStepCost;
                    turn = IFieldObject.MaxStepCost;
                }
                if (weightmap[v.x, v.y] != null) // если уже в этом тайле были 
                {
                     // если то же кол-во ходов, но стоит больше - то пропускаем тайл
                     if (weightmap[v.x, v.y].turn == turn &&
                         weightmap[v.x, v.y].cost < newTotalTurnCost) continue;
                     // раньше попадали в тайл за меньшее количество ходов, пропускаем
                     if (weightmap[v.x, v.y].turn < turn) continue; 
                     // в тайл пришли лучшим путем чем ранее, переписываем его и проверяем снова
                     weightmap[v.x, v.y] = null;
                     //continue;
                }

                weightmap[v.x, v.y] = new IcwStepWeigth(turn, newTotalTurnCost);
                // тайл проходимый и еще не смотрели его, добавляем в анализ
                if (turn != IFieldObject.MaxStepCost && !tilesToCheck.Contains(v)) 
                    tilesToCheck.Add(v);
            }
            return;
        }

        public IcwStepWeigth[,] GetWeightMap(IUnit acaller)
        {
            if (!(acaller is IFieldObject)) return null;
            caller = acaller;
            field = (caller as IFieldObject).Field;
            Vector2Int fieldsize = field.Size;
            Vector2Int objPosition = acaller.FieldPosition;
            weightmap = new IcwStepWeigth[fieldsize.x, fieldsize.y];
            tilesToCheck.Clear();
            tilesToCheck.Add(objPosition);
            weightmap[objPosition.x, objPosition.y] = new IcwStepWeigth(1, 0);
            int numiteration = 0;
            while (tilesToCheck.Count>0)
            {
                numiteration++;
                if (numiteration>1000)
                {
                    Debug.LogError("Calc Weight map cycling");
                    return null;
                }
                CheckOneTile();
                tilesToCheck.RemoveAt(0);
            }
            // если не все поле заполнили остаток добиваем непроходимыми клетками
            for (int x = 0; x < fieldsize.x; x++)
                for (int y = 0; y < fieldsize.y; y++)
                {
                    if (weightmap[x, y] == null) weightmap[x, y] = new IcwStepWeigth(IFieldObject.MaxStepCost, IFieldObject.MaxStepCost);
                }
            return weightmap;
        }

        public List<Vector2Int> GetPath(IUnit acaller, Vector2Int target, IcwStepWeigth[,] aweigths = null)
        {
            if (!(acaller is IFieldObject)) return null;
            int numiterations = 0;
            caller = acaller;
            field = (caller as IFieldObject).Field;
            if (aweigths == null) GetWeightMap(caller); // карты весов не было - делаем новую
            else weightmap = aweigths; // иначе берем готовую 
            List<Vector2Int> path = new List<Vector2Int>();

            if (!field.IsValidTileCoord(target)) return null;
            Vector2Int objPosition = acaller.FieldPosition;
            Vector2Int currpos = target;
            if (weightmap[currpos.x, currpos.y]!= null && weightmap[currpos.x, currpos.y].turn != IFieldObject.MaxStepCost)
                path.Add(currpos);
            while (currpos != objPosition && numiterations < 1000) 
            {
                numiterations++;
                if (numiterations > 1000) Debug.LogError("FindPath cycling!");
                Vector2Int[] neighbours = field.GetTileNeigbours(currpos);//GetAllNeigboursForHex(currpos.y % 2 == 1);
                int minturn = IFieldObject.MaxStepCost;
                int mincost = IFieldObject.MaxStepCost;
                Vector2Int newpos = new Vector2Int(-1, -1);
                foreach (Vector2Int n in neighbours)
                {
                    Vector2Int tmppos = n;// currpos + n;
                    if (!field.IsValidTileCoord(tmppos)) continue;
                    if (weightmap[tmppos.x, tmppos.y] == null) continue;
                    // ищем минимальную клетку - из равных берем рандомом чтобы разнообразнее ходили
                    if (weightmap[tmppos.x, tmppos.y].turn < minturn ||
                            (weightmap[tmppos.x, tmppos.y].turn == minturn && weightmap[tmppos.x, tmppos.y].cost < mincost) ||
                            // + рандом для одинаковых проходимых клеток
                            (
                                weightmap[tmppos.x, tmppos.y].turn != IFieldObject.MaxStepCost &&
                                weightmap[tmppos.x, tmppos.y].turn == minturn && 
                                weightmap[tmppos.x, tmppos.y].cost == mincost &&
                                Random.Range(0, 100) < 50
                            ))
                    {
                        if (!path.Contains(tmppos))
                        {
                            newpos = tmppos;
                            minturn = weightmap[tmppos.x, tmppos.y].turn;
                            mincost = weightmap[tmppos.x, tmppos.y].cost;
                        }
                    }
                }
                if (newpos != new Vector2Int(-1, -1))
                {
                    currpos = newpos;
                    path.Add(newpos);
                }
                else return null; // нет пути, возвращаем null
            } 
            return path;
        }
        public Vector2Int GetMinWeigthTileFromNeigbours(IcwStepWeigth[,] weigths, Vector2Int pos, int range)
        {
            List<Vector2Int> area = field.GetAreaInRange(pos, range);
            Vector2Int result = pos;

            foreach (Vector2Int v in area)
            {
                if (weigths[result.x, result.y].CompareTo(weigths[v.x, v.y]) > 0)
                    result = v;
            }
            return result;
        }

    }
}
