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
        void DoNextTurn(object acaller);
        void DoNextRound();
        delegate void BattleFlowEvent();
        event BattleFlowEvent OnNewRound;
    }
}
