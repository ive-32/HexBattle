using System.Collections.Generic;
using UnityEngine;
using TMPro;
using IcwBattle;
using IcwField;
using IcwUnits;

namespace IcwUI
{
    class IcwUIEvents : MonoBehaviour
    {
        public Camera gamecamera;
        public GameObject BattleObject;
        private IBattle battle;
        private IField field;
        private IPresenter presenter;
        private Vector2Int currentTile = Vector2Int.zero;
        private TextMeshProUGUI infotext;

        private void Awake()
        {
            BattleObject.TryGetComponent<IBattle>(out battle);
            if (IcwAtomFunc.IsNull(battle, this.name))
            {
                Destroy(this.gameObject);
                return;
            }
            BattleObject.TryGetComponent<IField>(out field);
            if (IcwAtomFunc.IsNull(field, this.name))
            {
                Destroy(this.gameObject);
                return;
            }

            BattleObject.TryGetComponent<IPresenter>(out presenter);
            if (IcwAtomFunc.IsNull(presenter, this.name))
            {
                Destroy(this.gameObject);
                return;
            }

            /*Transform trinfo = this.transform.Find("Info");
            if (trinfo != null)
            {
                trinfo.gameObject.TryGetComponent<TextMeshProUGUI>(out infotext);
            }*/
        }

        private void Update()
        {
            Vector3 mousepos = gamecamera.ScreenToWorldPoint(Input.mousePosition);
            mousepos.z = 0;

            if (Input.GetKey(KeyCode.Escape)) Application.Quit();
            if (Input.GetMouseButtonUp(0))
            {
                battle.OnClick(field.GetTileCoord(mousepos));
            }
            if (field.GetTileCoord(mousepos) != currentTile)
            {
                currentTile = field.GetTileCoord(mousepos);
                battle.OnMouseMove(currentTile);
                presenter.NeedUpdate = true;
            }

            // обновляем инфо
            if (field.IsValidTileCoord(currentTile))
            {
                IFieldObject underMouseUnit = field.battlefield[currentTile.x, currentTile.y].Find(o => o.ObjectType == IFieldObject.ObjType.Unit);
                presenter.PointedUnit = (IUnit)underMouseUnit;
                /*string infostring = "";
                if (underMouseUnit != null)
                    infostring = (underMouseUnit as IUnit).GetInfo();
                if (infotext != null) infotext.text = infostring;*/
            } 
        }

        public void ToggleShowCost()
        {
            IcwGlobalSettings.ShowStepCosts = !IcwGlobalSettings.ShowStepCosts;
            presenter.NeedUpdate = true;
        }

        public void EndTurn()
        {
            battle.DoEndTurn();
            presenter.NeedUpdate = true;
        }

    }
}
