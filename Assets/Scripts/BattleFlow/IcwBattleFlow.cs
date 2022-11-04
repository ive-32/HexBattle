using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps; 
using IcwField;
using IcwUnits;
using IcwUI;
using IcwAI;

namespace IcwBattle
{
    class IcwBattleFlow : MonoBehaviour, IBattle
    {
        enum BattleFlowState { BeforeTurn, InTurn }
        IBattle.BattleFlowActionMode IBattle.PlayerActionMode { get; set; } = IBattle.BattleFlowActionMode.MoveMode;
        public GameObject[] UnitPrefabs;
        public List<IcwFieldObjectType> listOfObjTypes;
        public Tilemap MainTileMap; // for test - now map generated from predefined tilemap
        public GameObject UnitLayer;
        public IField field;
        private IPresenter presenter = null;
        private int teamTurn = 1;
        private bool isBusy = false;
        private BattleFlowState state = BattleFlowState.BeforeTurn;
        private ITeamAI ai;

        public IFieldObject SelectedObject { get; set; } = null;
        IPresenter IBattle.Presenter { get => presenter; set => presenter = value; } 
        private IcwWeightMapGenerator WeightMapGenerator = new();

        private void Awake()
        {
            this.TryGetComponent<IField>(out field); // по другому в Unity не умею интерфесы привязывать. но не нравится метод
            if (IcwAtomFunc.IsNull(field, this.name))
                Destroy(this.gameObject);
            this.TryGetComponent<IPresenter>(out presenter);
            if (IcwAtomFunc.IsNull(presenter, this.name))
                Destroy(this.gameObject);

            this.TryGetComponent<ITeamAI>(out ai);
            if (IcwAtomFunc.IsNull(ai, this.name))
                Destroy(this.gameObject);
            
            ai.Battle = this;
            ai.Field = field;
            ai.Presenter = presenter;

        }

        private void Start()
        {
            //ai = new IcwTeamAI();
            
            IcwBattleFieldGenerator bg = new();
            bg.CreateMap(field, MainTileMap, listOfObjTypes);
            SetUnits();
            (this as IBattle).DoNextRound();
        }

        public void SetUnits()
        {
            // заглушка 
            // Команды: четные команда 1 нечетые команда 2
            string[] Names = { "Джо", "Рейчел", "Фиби", "Моника", "Чендлер", "Росс" };

            Vector2Int[] startTemplate = { 
                new Vector2Int(1, 1),
                new Vector2Int(field.Size.x - 2, field.Size.y - 2),
                new Vector2Int(0, 2),
                new Vector2Int(field.Size.x - 1, field.Size.y - 3),
                new Vector2Int(2, 0),
                new Vector2Int(field.Size.x - 3, field.Size.y - 1)
                };

            for (int i = 0; i < 6; i++)
            //for (int i = 0; i < 2; i++)
            {
                GameObject newUnitObject = Instantiate(UnitPrefabs[Random.Range(0, UnitPrefabs.Length)], UnitLayer.transform);
                newUnitObject.TryGetComponent<IUnit>(out IUnit unit);
                unit.team = i % 2;
                (unit as IFieldObject).Field = field;
                (unit as IFieldObject).Field.AddObject((unit as IFieldObject), startTemplate[i]);
                (unit as IUnit).WeightMapGenerator = WeightMapGenerator;
                (unit as IUnit).battle = this;
                (unit as IUnit).UnitName = $"{(unit.team == 0 ? "Красный " : "Зеленый ")} {(unit as IUnit).UnitName} {Names[i]}";
                if (unit.team % 2 == 0)
                    ai.Enemies.Add(unit as IUnit);
                else
                    ai.TeamMates.Add(unit as IUnit);
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
            /*if (unit.CurrentStats.TurnPoints <= 0)
            {
                SelectedObject = null;
                presenter.ShowText("ОД закончились. Передаем ход другой команде");
                (this as IBattle).DoNextTurn();
            }*/
        }

        IUnit DoSelectUnit(Vector2Int pos)
        {
            IUnit fieldObject = (IUnit)field.battlefield[pos.x, pos.y].Find(o => o is IUnit);// .ObjectType.FieldObjectTypeName == "Unit");

            // выбрали новый(другой) юнит, отправляем ему событие "тывыделен"
            if (state == BattleFlowState.BeforeTurn &&
                fieldObject != null &&
                fieldObject.team == teamTurn)
            {
                if (SelectedObject != fieldObject)
                {
                    if (!fieldObject.OnSelect())
                        fieldObject = null;
                }
                else // выбрали тот же юнит - снимаем выделение с него
                    fieldObject = null;
                SelectedObject = fieldObject;
                presenter.NeedUpdate = true;
            }
            return fieldObject;
        }

        void IBattle.OnClick(Vector2Int pos)
        {
            if (!field.IsValidTileCoord(pos)) return; // ткнули не в поле игнорируем
            if (isBusy) return;

            if (SelectedObject is IUnit unit && unit.team != teamTurn) SelectedObject = null;

            // проверяем что в тайле, если юнит запоминаем
            IUnit clickedUnit = DoSelectUnit(pos);
            
            // пытаемся ходить - если кликнули не в юнит или был выбран режим хождения
            if (SelectedObject is IUnit currUnit&&
                !(clickedUnit is IUnit)  &&
                (this as IBattle).PlayerActionMode == IBattle.BattleFlowActionMode.MoveMode)
            {
                bool success = currUnit.MoveByRoute(pos);
                if (success) state = BattleFlowState.InTurn;
                presenter.drawer.ClearTiles();
                presenter.NeedUpdate = true;
                return;
            }

            // вариант юнит был выбран и ткнули в чужой юнит атакуем
            if (SelectedObject is IUnit selectedunit&&
                ((clickedUnit != null &&
                selectedunit.team != clickedUnit.team)
                || 
                (this as IBattle).PlayerActionMode == IBattle.BattleFlowActionMode.AttackMode))
            {
                Vector2Int? result = selectedunit.DoAttack(pos); 
                if (result != null) state = BattleFlowState.InTurn;
                presenter.drawer.ClearTiles();
                presenter.NeedUpdate = true;
                return;
            }
        }

        
        void IBattle.DoNextRound()
        {
            if (isBusy) return;
            presenter.ShowText("--- Следующий раунд ---");
            presenter.ShowText("--- TP одновлены для всех юнитов ---");
            SelectedObject = null;
            Vector2Int fieldSize = field.Size;
            for (int x = 0; x < fieldSize.x; x++)
                for (int y = 0; y < fieldSize.y; y++)
                {
                    IFieldObject unit = field.battlefield[x, y].Find(o => o.ObjectType.FieldObjectTypeName == "Unit");
                    if (!(unit is IUnit)) continue;
                    (unit as IUnit).NewTurn();
                }
            teamTurn = 1;
            ai.DoNewRound();
            (this as IBattle).DoNextTurn();
        }

        void IBattle.DoNextTurn()
        {
            string[] teamnames = { "красные", "зеленые" };
            //if (isBusy) return;
            teamTurn++;
            teamTurn %= 2;
            presenter.ShowText($"--- Ход команды {teamnames[teamTurn % 2]}");
            SelectedObject = null;
            if (teamTurn % 2 == 1)
            {
                isBusy = true;
                ai.DoOneTurn();
                return;
            }
            else
            isBusy = false;

            presenter.ShowText("Выбирайте любого юнита для хода");
            presenter.NeedUpdate = true;
            state = BattleFlowState.BeforeTurn;
        }
    }
}
