using System.Collections.Generic;
using UnityEngine;
using IcwField;
using System.Collections;

namespace IcwUnits
{
    public interface IUnit : IFieldObject
    {
        int team { get; set; }
        IcwBattle.IBattle battle { get; set; }
        IcwUnitStats CurrentStats { get; set; }
        IcwUnitStats BaseStats { get; set; }
        IcwStepWeigth[,] weights { get; set; }
        List<Vector2Int> Route { get; set; }
        IcwWeightMapGenerator WeightMapGenerator { get; set; }
        IcwUnitBaseAttack Attack { get; set; }
        string GetInfo();
        int CostTile(List<IFieldObject> tileObjects);

        int DefaultCost(IFieldObject.ObjType objType)
        {
            return objType switch
            {
                IFieldObject.ObjType.Empty => IFieldObject.BaseStepCost,
                IFieldObject.ObjType.Grass => IFieldObject.BaseStepCost,
                IFieldObject.ObjType.Gravel => IFieldObject.BaseStepCost,
                IFieldObject.ObjType.Wall => IFieldObject.MaxStepCost,
                IFieldObject.ObjType.Unit => IFieldObject.MaxStepCost,
                _ => IFieldObject.BaseStepCost
            };
        }
        bool OnSelect();
        void OnMouseMove(Vector2Int pos);
        void MoveByRoute(Vector2Int pos);
        void NewTurn();
        void GetDamage(IcwUnitBaseAttack attack); 
    }
}
