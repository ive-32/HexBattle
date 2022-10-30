using System.Collections.Generic;
using IcwField;

namespace IcwUnits
{
    class IcwEvoqueUnit: IcwBaseUnit
    {
        /*public new int CostTile(List<IFieldObject> tileObject)
        {
            int cost = (this as IUnit).CostTile(tileObject);
            if (tileObject.Exists(o => o.ObjectType == IFieldObject.ObjType.Grass)) 
                cost = (cost * 60 / 100);
            if (tileObject.Exists(o => o.ObjectType == IFieldObject.ObjType.Gravel)) 
                cost = (cost * 100 / 60);
            return cost;
        }*/
    }
}
