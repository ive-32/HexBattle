using System.Collections.Generic;
using UnityEngine;

namespace IcwField
{

    public interface IFieldObject
    {
        const int BaseStepCost = 3;
        const int MaxStepCost = 1000000;
        IcwFieldObjectType ObjectType { get; set; }
        IField Field { get; set; } // поле которому принадлежит объект
        Vector2Int FieldPosition { get; set; }
    }
}
