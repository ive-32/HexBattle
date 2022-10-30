using System.Collections.Generic;
using UnityEngine;
using IcwField;
using IcwUnits;
using TMPro;

namespace IcwUI
{
    public class IcwPresenter : MonoBehaviour, IPresenter
    {
        public TextMeshProUGUI textout;
        public TextMeshProUGUI[] info;
        IField field;
        private IUnit pointedUnit;
        private IUnit selectedUnit;
        bool IPresenter.NeedUpdate { get; set; } = true;
        IUnit IPresenter.SelectedUnit 
        { 
            get => selectedUnit; 
            set 
            { 
                (this as IPresenter).NeedUpdate = (this as IPresenter).NeedUpdate || selectedUnit != value;
                selectedUnit = value;
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
        //void IPresenter.ShowTurnArea(IcwStepWeigth[,] weights) => field.ShowTurnArea(weights);

        //void IPresenter.ShowRoute(List<Vector2Int> route, IcwStepWeigth[,] weights) => field.ShowRoute(route, weights);

        void IPresenter.ShowText(string str)
        {
            string maintext = textout.text;
            string[] res = maintext.Split('\n');
            if (res.Length>5)
            {
                maintext = string.Join('\n', res, res.Length - 6, 5);
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
        }
        private void Update()
        {
            if (!(this as IPresenter).NeedUpdate) return;
            if (selectedUnit != null)
            {
                field.ShowTurnArea(selectedUnit.weights);
                field.ShowRoute(selectedUnit.Route, selectedUnit.weights);
                (this as IPresenter).ShowInfo(selectedUnit.GetInfo(), 0);
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
