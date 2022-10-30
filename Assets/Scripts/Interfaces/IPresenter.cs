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
        //void ShowTurnArea(IcwStepWeigth[,] weights);
        //void ShowRoute(List<Vector2Int> route, IcwStepWeigth[,] weights);
        void ShowText(string str);
        void ShowInfo(string str, int InfoWindowNumber);

    }
}
