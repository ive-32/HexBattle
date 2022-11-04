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

            
        }

        Vector2Int position1 = new Vector2Int(0, 0);


        private void Update()
        {
            //TestDistance(); return; // тестировал расчет расстояния 
            //TestViewRange(); return;
            //TestTilesOnLine();  return; // тестировал как строит линию между двумя тайлами
            //TestIsVisibleFrom();  return; // тестировал как строит линию между двумя тайлами
            //TestViewVisibleRange(); return; // тестировал как показывает visbile range

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
                presenter.OnMouseMove(currentTile);
                
            }

            // обновляем инфо
            if (field.IsValidTileCoord(currentTile))
            {
                IFieldObject underMouseUnit = field.battlefield[currentTile.x, currentTile.y].Find(o => o.ObjectType.FieldObjectTypeName == "Unit");
                presenter.PointedUnit = (IUnit)underMouseUnit;
                
            } 
        }

        public void ToggleShowCost()
        {
            IcwGlobalSettings.ShowStepCosts = !IcwGlobalSettings.ShowStepCosts;
            presenter.NeedUpdate = true;
        }

        public void EndTurn()
        {
            battle.DoNextTurn();
            presenter.NeedUpdate = true;
        }

        public void NextRound()
        {
            battle.DoNextRound();
            presenter.NeedUpdate = true;
        }

        void TestDistance()
        {
            // test distance
            // кликаем в поле - считает расстояние между последними двумя кликами
            Vector3 mousepos = gamecamera.ScreenToWorldPoint(Input.mousePosition);
            mousepos.z = 0;

            if (Input.GetMouseButtonUp(0))
            {
                int dist = 0;
                Vector2Int pos = field.GetTileCoord(mousepos);
                if (field.IsValidTileCoord(pos))
                {
                    if (position1 != pos)
                        dist = field.Distance(position1, pos);
                    //field.ShowCoordTile(true);
                    presenter.ShowText($"dist {dist} points {position1}, {pos}");
                    presenter.drawer.ShowBorderTile(position1, 1);
                    presenter.drawer.ShowBorderTile(pos, 1);
                    position1 = pos;
                }
                presenter.NeedUpdate = false;
            }
            return;
        }

        void TestIsVisibleFrom()
        {
            // test IsVisibleFrom
            // кликаем в поле - рисует линию до первого препятствия
            Vector3 mousepos = gamecamera.ScreenToWorldPoint(Input.mousePosition);
            mousepos.z = 0;

            if (Input.GetMouseButtonUp(0))
            {
                Vector2Int pos = field.GetTileCoord(mousepos);
                if (field.IsValidTileCoord(pos))
                {
                    //field.ShowCoordTile(true);
                    if (position1 != pos)
                    {
                        List<Vector2Int> line;
                        bool IsVisible = field.IsTileVisibleFrom(position1, pos, out line);
                        foreach (Vector2Int v in line)
                            presenter.drawer.ShowBorderTile(v, 1);
                        presenter.ShowText($"isVisivle? from {position1} to {pos} is {IsVisible}");
                    }
                    position1 = pos;
                }
                presenter.NeedUpdate = false;
            }
            return;
        }

        void TestTilesOnLine()
        {
            // test tile on line
            // кликаем в поле - рисует линию  между последними двумя кликами
            Vector3 mousepos = gamecamera.ScreenToWorldPoint(Input.mousePosition);
            mousepos.z = 0;

            if (Input.GetMouseButtonUp(0))
            {
                Vector2Int pos = field.GetTileCoord(mousepos);
                if (field.IsValidTileCoord(pos))
                {
                    //field.ShowCoordTile(true);
                    if (position1 != pos)
                    {
                        List<Vector2Int> line = field.GetTilesOnLine(position1, pos);
                        foreach (Vector2Int v in line)
                            presenter.drawer.ShowBorderTile(v, 1);
                        presenter.ShowText($"num tiles {line.Count} points {position1}, {pos}");
                    }
                    position1 = pos;
                }
                presenter.NeedUpdate = false;
            }
            return;
        }

        void TestViewRange()
        {
            // test ViewRange
            // кликаем в поле - показывает область которая внутри случайно выбранной величины от 2 до 5 
            Vector3 mousepos = gamecamera.ScreenToWorldPoint(Input.mousePosition);
            mousepos.z = 0;

            if (Input.GetMouseButtonUp(0))
            {
                int dist = 3; // Random.Range(2, 6);
                Vector2Int pos = field.GetTileCoord(mousepos);
                if (field.IsValidTileCoord(pos))
                {
                    List<Vector2Int> viewrangelist = field.GetAreaInRange(pos, dist);
                    //field.ShowCoordTile(true);
                    presenter.ShowText($"dist {dist} points {pos}");
                    foreach(Vector2Int viewpos in viewrangelist)
                        presenter.drawer.ShowBorderTile(viewpos, 1);
                }
                presenter.NeedUpdate = false;
            }
            return;
        }

        void TestViewVisibleRange()
        {
            // test ViewVisibleRange
            // кликаем в поле - показывает область которая ВИДИМА внутри случайно выбранной величины от 2 до 4 
            Vector3 mousepos = gamecamera.ScreenToWorldPoint(Input.mousePosition);
            mousepos.z = 0;

            if (Input.GetMouseButtonUp(0))
            {
                int dist = 7; // фиксируем для измерения скорости Random.Range(2, 7);
                Vector2Int pos = field.GetTileCoord(mousepos);
                if (field.IsValidTileCoord(pos))
                {
                    List<Vector2Int> viewrangelist = field.GetVisibleTiles(pos, dist);
                    presenter.ShowText($"{viewrangelist.Count} dist {dist} points {pos}");
                    foreach (Vector2Int viewpos in viewrangelist)
                        presenter.drawer.ShowBorderTile(viewpos, 1);
                    presenter.drawer.ShowBorderTile(pos, 4);
                }
                presenter.NeedUpdate = false;
            }
            return;
        }
    }
}
