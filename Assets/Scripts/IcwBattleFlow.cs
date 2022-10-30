using System.Collections.Generic;
using UnityEngine;
using IcwField;
using IcwUnits;
using IcwUI;

namespace IcwBattle
{
    class IcwBattleFlow : MonoBehaviour, IBattle
    {
        public GameObject[] UnitPrefabs;
        public GameObject UnitLayer;
        public IField field;
        private IPresenter presenter = null;
        public IFieldObject SelectedObject { get; set; } = null;
        //public IFieldObject MovingObject { get; set; } = null;
        IPresenter IBattle.Presenter { get => presenter; set => presenter = value; } 
        private IcwWeightMapGenerator WeightMapGenerator = new();
        private bool isBusy = false;

        private void Awake()
        {
            this.TryGetComponent<IField>(out field); // по другому в Unity не умею интерфесы привязывать. но не нравится метод
            if (IcwAtomFunc.IsNull(field, this.name))
                Destroy(this.gameObject);
            this.TryGetComponent<IPresenter>(out presenter);
            if (IcwAtomFunc.IsNull(presenter, this.name))
                Destroy(this.gameObject);

        }

        private void Start()
        {
            SetUnits();
        }

        public void SetUnits()
        {
            // заглушка 
            // Команды: четные команда 1 нечетые команда 2
            Vector2Int[] startTemplate = { 
                new Vector2Int(1, 1),
                new Vector2Int(field.GetSize.x - 2, field.GetSize.y - 2),
                new Vector2Int(0, 2),
                new Vector2Int(field.GetSize.x - 1, field.GetSize.y - 3),
                new Vector2Int(2, 0),
                new Vector2Int(field.GetSize.x - 3, field.GetSize.y - 1)
                };

            for (int i = 0; i < 6; i++)
            {
                GameObject newUnitObject = Instantiate(UnitPrefabs[Random.Range(0, UnitPrefabs.Length)], UnitLayer.transform);
                newUnitObject.TryGetComponent<IUnit>(out IUnit unit);
                unit.team = i % 2;
                (unit as IFieldObject).Field = field;
                (unit as IFieldObject).Field.AddObject((unit as IFieldObject), startTemplate[i]);
                (unit as IUnit).WeightMapGenerator = WeightMapGenerator;
                (unit as IUnit).battle = this;
            }
        }

        void IBattle.UnitActionStart(IcwUnits.IUnit unit)
        {
            // позже придется превратить в лист - несколько объктов дают визуал - добавляем их
            // когда заканчивают убираем - 
            // игровая логика включается только тогда когда нет ни одного объекта в листе визуала
            isBusy = true;
        }

        void IBattle.UnitActionComplete(IcwUnits.IUnit unit)
        {
            isBusy = false;
        }

        void IBattle.OnClick(Vector2Int pos)
        {
            if (!field.IsValidTileCoord(pos)) return; // ткнули не в поле игнорируем
            if (isBusy) return;
            // проверяем что в тайле если юнит запоминаем
            IFieldObject fieldObject = field.battlefield[pos.x, pos.y].Find(o => o.ObjectType == IFieldObject.ObjType.Unit);

            // выбрали новый(другой) юнит, отправляем ему событие "тывыделен"
            if (fieldObject is IUnit &&
                SelectedObject != fieldObject &&
                    (
                        SelectedObject == null ||
                        (
                            SelectedObject is IUnit &&
                            (SelectedObject as IUnit).team == (fieldObject as IUnit).team
                        )
                    )
                )
            {
                // дописать чтобы выбирал только своих, пока выбираем всех для отладки
                (fieldObject as IUnit).OnSelect();
                SelectedObject = fieldObject;
                presenter.SelectedUnit = (IUnit)SelectedObject;
                return;
            }

            // выбрали тот же юнит - снимаем выделение с него
            if (SelectedObject == fieldObject)
            {
                SelectedObject = null;
                presenter.SelectedUnit = (IUnit)SelectedObject;
                return;
            }

            // вариант юнит был выбран и ткнули в поле - идем
            if (SelectedObject is IUnit && !(fieldObject is IUnit))
            {
                (SelectedObject as IUnit).MoveByRoute(pos);
                presenter.NeedUpdate = true;
                return;
            }

            // вариант юнит был выбран и ткнули в чужой юнит атакуем
            if (SelectedObject is IUnit &&
                fieldObject is IUnit &&
                (SelectedObject as IUnit).team != (fieldObject as IUnit).team)
            {
                (SelectedObject as IUnit).Attack.DoDamage(fieldObject as IUnit);
                presenter.NeedUpdate = true;
                return;
            }
        }

        void IBattle.OnMouseMove(Vector2Int pos)
        {
            if (isBusy) return;
            if (SelectedObject == null) return;
            if (SelectedObject is IUnit && pos != SelectedObject.FieldPosition)
                (SelectedObject as IUnit).OnMouseMove(pos);
            
        }

        void IBattle.DoEndTurn()
        {
            presenter.ShowText("--- Следующий ход ---");
            if (isBusy) return;
            SelectedObject = null;
            //MovingObject = null;
            Vector2Int fieldSize = field.GetSize;
            for (int x = 0; x < fieldSize.x; x++)
                for (int y = 0; y < fieldSize.y; y++)
                {
                    IFieldObject unit = field.battlefield[x, y].Find(o => o.ObjectType == IFieldObject.ObjType.Unit);
                    if (!(unit is IUnit)) continue;
                    (unit as IUnit).NewTurn();
                }
        }
    }
}
