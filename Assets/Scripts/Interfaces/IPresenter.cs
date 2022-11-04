using System.Collections.Generic;
using UnityEngine;
using IcwField;
using IcwUnits;

namespace IcwUI
{
    public interface IPresenter
    {
        bool NeedUpdate { get; set; }
        IUnit SelectedUnit { get; set; }
        IUnit PointedUnit { get; set; }
        public IcwTileDrawer drawer { get; set; }
        void ShowText(string str);
        void ShowInfo(string str, int InfoWindowNumber);
        void OnMouseMove(Vector2Int pos);
    }
}
