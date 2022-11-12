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
        public GameObject TextPerlin;
        public event IBattle.BattleFlowEvent OnNewRound;


        private IPresenter presenter = null;
        private int teamTurn = 1;
        private List<object> ActiveObjects = new List<object>();
        private bool isBusy { get => ActiveObjects.Count > 0; }
        private BattleFlowState state = BattleFlowState.BeforeTurn;
        private ITeamAI ai;
        private int BaseTurnCountOnRound;
        private int CurrentTurnCountOnRound;


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

            
            ai = new IcwTeamAI();
            ai.Battle = this;
            ai.Field = field;
            ai.Presenter = presenter;
            ai.OnAIActionStart += OnVisualStart;
            ai.OnAIActionEnd += OnVisualEnd;

            //GenerateByPerlin();
        }

        private void Start()
        {
            //ai = new IcwTeamAI();
            

            IcwBattleFieldGenerator bg = new();
            SpriteRenderer sr = TextPerlin.GetComponent<SpriteRenderer>();
            Texture2D tex = bg.CreateByPerlin(field, MainTileMap, listOfObjTypes);
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero, tex.width);
            // bg.CreateMap(field, MainTileMap, listOfObjTypes);
            presenter.ShowText("BF: Создали карту");
            SetUnits();
            presenter.ShowText("BF: Расставили юнитов");
            BaseTurnCountOnRound = Mathf.CeilToInt(UnitLayer.transform.childCount / 2.0f);
            presenter.ShowText($"BF: Количество ходов за круг для каждой команды {BaseTurnCountOnRound}");
            CurrentTurnCountOnRound = BaseTurnCountOnRound * 2; // сразу запускаем первый раунд
            (this as IBattle).DoNextTurn(null);

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
            {
                GameObject newUnitObject = Instantiate(UnitPrefabs[Random.Range(0, UnitPrefabs.Length)], UnitLayer.transform);
                newUnitObject.TryGetComponent<IUnit>(out IUnit unit);
                unit.team = i % 2;
                (unit as IFieldObject).Field = field;
                (unit as IFieldObject).Field.AddObject((unit as IFieldObject), startTemplate[i]);
                (unit as IUnit).WeightMapGenerator = WeightMapGenerator;
                (unit as IUnit).Battle = this;
                (unit as IUnit).UnitName = $"{(unit.team == 0 ? "Красный " : "Зеленый ")} {(unit as IUnit).UnitName} {Names[i]}";
                (unit as IUnit).VisualActionStart += OnVisualStart;
                (unit as IUnit).VisualActionEnd += OnVisualEnd;
                ai.AddUnit(unit as IUnit, unit.team % 2 == 0 ? ITeamAI.AITeamType.Enemies : ITeamAI.AITeamType.Mates);
            }
        }

        void OnVisualStart(object unit)
        {
            ActiveObjects.Add(unit);
        }

        void OnVisualEnd(object unit)
        {
            if (!ActiveObjects.Contains(unit))
                throw new System.Exception($"unit {unit} wasn't active but send Iam inactive");
            ActiveObjects.Remove(unit);
        }


        IUnit DoSelectUnit(Vector2Int pos)
        {
            IUnit unit = (IUnit)field.battlefield[pos.x, pos.y].Find(o => o is IUnit);// .ObjectType.FieldObjectTypeName == "Unit");

            if (state == BattleFlowState.BeforeTurn &&
                unit != null &&
                unit.team == teamTurn)
            {
                // выбрали тот же юнит, или юнит занят в визуале - снимаем выделение с него
                if (SelectedObject == unit || !unit.IsAvailable())
                    unit = null;
                SelectedObject = unit;
                presenter.NeedUpdate = true;
            }
            return unit;
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
                //presenter.drawer.ClearTiles();
                presenter.NeedUpdate = true;
                return;
            }

            // вариант юнит был выбран и ткнули в чужой юнит атакуем
            if (SelectedObject is IUnit selectedunit&&
                ((clickedUnit != null &&
                selectedunit.team != clickedUnit.team) || 
                (this as IBattle).PlayerActionMode == IBattle.BattleFlowActionMode.AttackMode))
            {
                Vector2Int? result = selectedunit.DoAttack(pos); 
                if (result != null) state = BattleFlowState.InTurn;
                //presenter.drawer.ClearTiles();
                presenter.NeedUpdate = true;
                return;
            }
        }

        
        void IBattle.DoNextRound()
        {
            if (isBusy) return;
            presenter.ShowText("BF: ----------- Раунд ----------------");
            presenter.ShowText("BF: --- TP одновлены для всех юнитов ---");
            SelectedObject = null;
            CurrentTurnCountOnRound = 1;
            teamTurn = 1;
            OnNewRound?.Invoke();
        }

        void IBattle.DoNextTurn(object acaller)
        {

            CurrentTurnCountOnRound++;
            if (CurrentTurnCountOnRound > BaseTurnCountOnRound * 2)
                (this as IBattle).DoNextRound();
            if (CurrentTurnCountOnRound > 6)
                throw new System.Exception($"Turn 7 not switch the round {isBusy}");
            presenter.ShowText($"BF: Ход в раунде {CurrentTurnCountOnRound} ");

            teamTurn++;
            teamTurn %= 2;
            
            presenter.ShowText($"BF: Ход {(CurrentTurnCountOnRound + 1)/ 2} команды {(teamTurn % 2 == 0 ? "красные" : "зеленые")}");
            SelectedObject = null;
            if (teamTurn % 2 == 1)
            {
                ai.DoOneTurn();
                return;
            }

            presenter.ShowText("BF: Выбирайте любого юнита для хода");
            presenter.NeedUpdate = true;
            state = BattleFlowState.BeforeTurn;
        }
    }
}
