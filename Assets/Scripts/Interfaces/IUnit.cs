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
        IcwUnitBaseAttackAbility AttackAbility { get; set; }
        string GetInfo();
        int CostTile(List<IFieldObject> tileObjects);

        int DefaultCost(IcwFieldObjectType objType) => objType.IsMoveObstacle ? IFieldObject.MaxStepCost : IFieldObject.BaseStepCost;
        bool OnSelect();
        void OnMouseMove(Vector2Int pos);
        bool MoveByRoute(Vector2Int pos);
        void NewTurn();
        Vector2Int? DoAttack(Vector2Int target);
    }
}
