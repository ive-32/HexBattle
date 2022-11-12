using UnityEngine;
using IcwField;

namespace IcwUnits
{
    class IcwDeadUnit : MonoBehaviour, IFieldObject
    {
        [SerializeField] private IcwFieldObjectType objectType;

        public IcwFieldObjectType ObjectType { get => objectType; set => objectType = value; }
        public IField Field { get; set; }
        public Vector2Int FieldPosition { get; set; }
    }
}
