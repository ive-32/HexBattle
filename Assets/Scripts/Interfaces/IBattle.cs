using System.Collections.Generic;
using UnityEngine;
using IcwUI;
using IcwField;

namespace IcwBattle
{
    public interface IBattle
    {
        enum BattleFlowActionMode { AttackMode, MoveMode };
        BattleFlowActionMode PlayerActionMode { get; set; }
        IPresenter Presenter { get; set; }
        IFieldObject SelectedObject { get; set; }
        void OnClick(Vector2Int pos);
        void OnMouseMove(Vector2Int pos);
        void DoNextTurn();
        void DoNextRound();
        void UnitActionStart(IcwUnits.IUnit unit);
        void UnitActionComplete(IcwUnits.IUnit unit);
    }
}
