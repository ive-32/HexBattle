using System.Collections.Generic;
using UnityEngine;
using IcwUI;

namespace IcwBattle
{
    public interface IBattle
    {
        IPresenter Presenter { get; set; }
        void OnClick(Vector2Int pos);
        void OnMouseMove(Vector2Int pos);
        void DoEndTurn();
        void UnitActionStart(IcwUnits.IUnit unit);
        void UnitActionComplete(IcwUnits.IUnit unit);
    }
}
