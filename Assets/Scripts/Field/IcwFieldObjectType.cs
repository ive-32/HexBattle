using System;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

namespace IcwField
{
    [CreateAssetMenu(fileName = "New FieldObjectType", menuName = "FieldObjectTypes", order = 51)]
    public class IcwFieldObjectType : ScriptableObject
    {
        public string FieldObjectTypeName;
        public bool IsViewObstacle;
        public bool IsMoveObstacle;
        public List<TileBase> TileSprite;
    }
}
