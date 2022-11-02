using System.Collections.Generic;
using UnityEngine;

namespace IcwField
{

    public interface IFieldObject
    {
        const int BaseStepCost = 3;
        const int MaxStepCost = 1000000;
        enum ObjType {Empty, Grass, Gravel, Wall, Unit }
        //ObjType ObjectType { get; set; }
        IcwFieldObjectType ObjectType { get; set; }
        IField Field { get; set; } // поле которому принадлежит объект
        Vector2Int FieldPosition { get; set; }
    }
}
