using System.Collections.Generic;
using UnityEngine;
using IcwUnits;

namespace IcwField
{
    public interface IField
    {
        Vector2Int GetSize { get; }
        List<IFieldObject>[,] battlefield { get; set; }
        Vector3 GetWorldCoord(Vector2Int tilecoord);
        Vector2Int GetTileCoord(Vector3 worldPosition);
        bool IsValidTileCoord(Vector2Int pos);
        void RemoveObject(IFieldObject obj);
        void AddObject(IFieldObject obj, Vector2Int pos);
        void MoveObject(IFieldObject obj, Vector2Int newpos);
        //Vector2Int GetObjectPosition(IFieldObject obj);
        bool IsObjectInTile(IFieldObject obj, Vector2Int pos);
        void ShowTurnArea(IcwStepWeigth[,] weights);
        void ShowRoute(List<Vector2Int> route, IcwStepWeigth[,] weights);

    }
}
