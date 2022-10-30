using System.Collections.Generic;
using IcwField;

namespace IcwUnits
{
    class IcwTrooperUnit : IcwBaseUnit
    {
        public override int CostTile(List<IFieldObject> tileObject)
        {
            int cost = base.CostTile(tileObject);
            if (cost == IFieldObject.MaxStepCost) return cost;
            if (tileObject.Exists(o => o.ObjectType == IFieldObject.ObjType.Grass)) cost = 4;
            if (tileObject.Exists(o => o.ObjectType == IFieldObject.ObjType.Gravel)) cost = 1;
            return cost;
        }
    }
    

}
