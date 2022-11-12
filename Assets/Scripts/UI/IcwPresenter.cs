using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using IcwField;
using IcwUnits;
using IcwBattle;
using TMPro;

namespace IcwUI
{
    public class IcwPresenter : MonoBehaviour, IPresenter
    {
        public List<TileBase> TileBaseObjects = new List<TileBase>();
        public Tilemap InfoTileMap;
        public GameObject TextPrefab;
        public TextMeshProUGUI textout;
        public TextMeshProUGUI[] info;
        IField field;
        IBattle battle;
        private IUnit pointedUnit;
        public IcwTileDrawer drawer { get; set; }
        bool IPresenter.NeedUpdate { get; set; } = true;
        private IUnit activeUnit;
        IUnit IPresenter.PointedUnit
        { 
            get => pointedUnit;
            set
            {
                (this as IPresenter).NeedUpdate = (this as IPresenter).NeedUpdate || pointedUnit != value;
                pointedUnit = value;
            }
        }

        void ActiveUnitStatsChanged(object obj)
        {
            if (!(obj is IUnit unit)) return;
            unit.weights = null;
            unit.Route = null;
            (this as IPresenter).NeedUpdate = true;
        }

        void IPresenter.ShowText(string str)
        {
            string maintext = textout.text;
            string[] res = maintext.Split('\n');
            if (res.Length>100)
            {
                maintext = string.Join('\n', res, res.Length - 100, 100);
            }
            textout.text = maintext + "\n" + str;
        }
        void IPresenter.ShowInfo(string str, int InfoWindowNumber)
        {
            info[InfoWindowNumber].text = (InfoWindowNumber == 0 ? "ActiveUnit\n": "Pointed Unit\n") + str;
        }

        void IPresenter.OnMouseMove(Vector2Int pos)
        {
            if (!field.IsValidTileCoord(pos)) return;
            if (activeUnit is IUnit)
            {
                if (!activeUnit.IsAvailable() || pos == activeUnit.FieldPosition) return;
                if (activeUnit.weights == null)
                    activeUnit.weights = activeUnit.WeightMapGenerator.GetWeightMap(activeUnit);
                activeUnit.Route = activeUnit.WeightMapGenerator.GetPath(activeUnit, pos, activeUnit.weights);
                (this as IPresenter).NeedUpdate = true;
            }

            IUnit unitUnderMouse = (IUnit)field.battlefield[pos.x, pos.y].Find(o => o.ObjectType.FieldObjectTypeName == "Unit");
            if (unitUnderMouse != pointedUnit)
            {
                pointedUnit = unitUnderMouse;
                (this as IPresenter).NeedUpdate = true;
            }
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
            drawer = new IcwTileDrawer(field, InfoTileMap);
            drawer.TilePrefabs = TileBaseObjects;
            drawer.TextPrefab = TextPrefab;
        }

        private void Update()
        {
            if (!(this as IPresenter).NeedUpdate) return;
            if (battle.SelectedObject != activeUnit)
            {
                if (activeUnit is IUnit)
                    activeUnit.VisualActionEnd -= ActiveUnitStatsChanged;
                if (battle.SelectedObject is IUnit)
                {
                    activeUnit = battle.SelectedObject as IUnit;
                    activeUnit.VisualActionEnd += ActiveUnitStatsChanged;
                }
                else
                    activeUnit = null;
            }
            if (activeUnit is IUnit)
            {
                if (activeUnit.weights == null)
                    activeUnit.weights = activeUnit.WeightMapGenerator.GetWeightMap(activeUnit);

                if (activeUnit.IsAvailable())
                {
                    if (activeUnit.Route != null)
                        drawer.ShowRoute(activeUnit.Route, activeUnit.weights);
                    else
                        drawer.ShowTurnArea(activeUnit.weights);
                }
                (this as IPresenter).ShowInfo(activeUnit.GetInfo(), 0);
            }
            else
            {
                drawer.ClearTiles();
                (this as IPresenter).ShowInfo("", 0);
            }



            if (pointedUnit is IUnit)
                (this as IPresenter).ShowInfo(pointedUnit.GetInfo(), 1);
            else 
                (this as IPresenter).ShowInfo("", 1);
            (this as IPresenter).NeedUpdate = false;
        }
    }
}
