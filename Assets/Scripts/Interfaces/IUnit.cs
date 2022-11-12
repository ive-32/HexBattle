using System.Collections.Generic;
using UnityEngine;
using IcwField;
using System.Collections;

namespace IcwUnits
{
    public interface IUnit : IFieldObject
    {
        int team { get; set; }
        string UnitName { get; set; }
        IcwBattle.IBattle Battle { get; set; }
        IcwUnitStats CurrentStats { get; set; }
        IcwUnitStats BaseStats { get; set; }
        IcwStepWeigth[,] weights { get; set; }
        List<Vector2Int> Route { get; set; }
        IcwWeightMapGenerator WeightMapGenerator { get; set; }
        IcwUnitBaseAttackAbility AttackAbility { get; set; }
        bool IsAvailable();
        string GetInfo();
        int CostTile(List<IFieldObject> tileObjects);
        int DefaultCost(IcwFieldObjectType objType) => objType.IsMoveObstacle ? IFieldObject.MaxStepCost : IFieldObject.BaseStepCost;
        bool MoveByRoute(Vector2Int pos);
        Vector2Int? DoAttack(Vector2Int target);

        delegate void UnitEvent(object unit);
        event UnitEvent VisualActionStart;
        event UnitEvent VisualActionEnd;
        event UnitEvent UnitDead;

    }
}
