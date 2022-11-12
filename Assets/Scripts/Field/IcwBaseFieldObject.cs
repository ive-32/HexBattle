using System.Collections.Generic;
using UnityEngine;

namespace IcwField
{
    class IcwBaseFieldObject : IFieldObject
    {
        IcwFieldObjectType IFieldObject.ObjectType { get; set; } 
        IField IFieldObject.Field { get; set; } = null; // Поле на котором юнит находится

        Vector2Int IFieldObject.FieldPosition { get; set; }
        
    }
}
