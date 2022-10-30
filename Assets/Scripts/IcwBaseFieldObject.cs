using System.Collections.Generic;
using UnityEngine;

namespace IcwField
{
    class IcwBaseFieldObject : IFieldObject
    {
        IFieldObject.ObjType IFieldObject.ObjectType { get; set; } = IFieldObject.ObjType.Empty;
        IField IFieldObject.Field { get; set; } = null; // Поле на котором юнит находится
        public IcwBaseFieldObject() { }
        public IcwBaseFieldObject(IFieldObject.ObjType aObjType) => (this as IFieldObject).ObjectType = aObjType;

        Vector2Int IFieldObject.FieldPosition { get; set; }
        
    }
}
