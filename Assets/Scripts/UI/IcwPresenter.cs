using UnityEngine;
using IcwField;
using IcwUnits;
using IcwBattle;
using TMPro;

namespace IcwUI
{
    public class IcwPresenter : MonoBehaviour, IPresenter
    {
        public TextMeshProUGUI textout;
        public TextMeshProUGUI[] info;
        IField field;
        IBattle battle;
        private IUnit pointedUnit;
        bool IPresenter.NeedUpdate { get; set; } = true;
        IUnit IPresenter.SelectedUnit 
        {
            get 
            {
                if (battle.SelectedObject is IUnit) return battle.SelectedObject as IUnit;
                else return null;
            }
            set 
            {   
                (this as IPresenter).NeedUpdate = (this as IPresenter).NeedUpdate || battle.SelectedObject != value;
                battle.SelectedObject = value;
            } 
        }
        IUnit IPresenter.PointedUnit
        { 
            get => pointedUnit;
            set
            {
                (this as IPresenter).NeedUpdate = (this as IPresenter).NeedUpdate || pointedUnit != value;
                pointedUnit = value;
            }
        }

        void IPresenter.ShowText(string str)
        {
            string maintext = textout.text;
            string[] res = maintext.Split('\n');
            if (res.Length>3)
            {
                maintext = string.Join('\n', res, res.Length - 3, 3);
            }
            textout.text = maintext + "\n" + str;
        }
        void IPresenter.ShowInfo(string str, int InfoWindowNumber)
        {
            info[InfoWindowNumber].text = (InfoWindowNumber == 0 ? "ActiveUnit\n": "Pointed Unit\n") + str;
        }

        private void Awake()
        {
            textout.text = "Presenter started";
            this.TryGetComponent<IField>(out field);
            if (field == null)
            {
                (this as IPresenter).ShowText("Field object not founded");
                Destroy(this.gameObject);
            }
            this.TryGetComponent<IBattle>(out battle);
            if (battle == null)
            {
                (this as IPresenter).ShowText("Battle object not founded");
                Destroy(this.gameObject);
            }

        }
        private void Update()
        {
            if (!(this as IPresenter).NeedUpdate) return;
            if ((this as IPresenter).SelectedUnit is IUnit)
            {
                IUnit unit = (this as IPresenter).SelectedUnit;
                field.ShowTurnArea(unit.weights);
                field.ShowRoute(unit.Route, unit.weights);
                (this as IPresenter).ShowInfo(unit.GetInfo(), 0);
            }
            else
            {
                field.ShowTurnArea(null);
                (this as IPresenter).ShowInfo("", 0);
            }
            if (pointedUnit!=null)
                (this as IPresenter).ShowInfo(pointedUnit.GetInfo(), 1);
            else 
                (this as IPresenter).ShowInfo("", 1);
        }
    }
}
