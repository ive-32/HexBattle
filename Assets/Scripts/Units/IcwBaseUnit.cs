using System.Collections.Generic;
using UnityEngine;
using IcwField;
using System.Collections;
using IcwBattle;

namespace IcwUnits
{
    class IcwBaseUnit : MonoBehaviour, IUnit , IDamageable 
    {

        // Реализация интерфейса IFieldObject
        IcwFieldObjectType IFieldObject.ObjectType{ get; set; }
        IField IFieldObject.Field { get; set; } = null; // Поле на котором юнит находится
        Vector2Int IFieldObject.FieldPosition
        {
            get => (this as IFieldObject).Field.GetTileCoord(this.transform.position); 
            set
            {
                if (!(this as IFieldObject).Field.IsValidTileCoord(value)) return;
                this.transform.position = (this as IFieldObject).Field.GetWorldCoord(value);
            }
        }
        // реализация интерфейса IUnit
        public string UnitName { get; set; }
        IcwWeightMapGenerator IUnit.WeightMapGenerator { get; set; }
        IcwUnitStats IUnit.CurrentStats { get; set; } = new IcwUnitStats();
        IcwUnitStats IUnit.BaseStats { get; set; } = new IcwUnitStats();
        int IUnit.team { get; set; } = 0;
        private IBattle battle = null;
        IBattle IUnit.Battle
        {
            get => battle; 
            set
            {
                if (battle != null)
                    battle.OnNewRound -= NewRound;
                battle = value;
                battle.OnNewRound += NewRound;
            }
        }
        IcwStepWeigth[,] IUnit.weights { get; set; }
        List<Vector2Int> IUnit.Route { get; set; }
        IcwUnitBaseAttackAbility IUnit.AttackAbility { get; set; } = new IcwUnitBaseAttackAbility();
        public event IUnit.UnitEvent VisualActionStart;
        public event IUnit.UnitEvent VisualActionEnd;
        public event IUnit.UnitEvent UnitDead;

        protected void RaiseVisualActionStart(IUnit unit) => VisualActionStart?.Invoke(this);
        protected void RaiseVisualActionEnd(IUnit unit) => VisualActionEnd?.Invoke(this);
        protected void RaiseVisualActionUnitDead(IUnit unit) => UnitDead?.Invoke(this);
        // реализация интерфейса IUnit

        GameObject UnitSprite;

        public Transform[] teamcolor = new Transform[2];
        public IcwFieldObjectType UnitFieldObjectType;
        protected bool IsBusyByVisual = false;
        public GameObject DeadUnitPrefab;
       

        // визуал, все что с ним связано
        protected virtual void Awake()
        {
            if (IcwAtomFunc.IsNull(this.transform.Find("MainSprite"), this.name))
            {
                Destroy(this.gameObject);
                return;
            }
            UnitSprite = this.transform.Find("MainSprite").gameObject;
            (this as IUnit).BaseStats.TurnPoints = Random.Range(15, 20);
            (this as IUnit).BaseStats.Health = Random.Range(90, 115);
            (this as IUnit).CurrentStats.SetStats((this as IUnit).BaseStats);
            (this as IUnit).AttackAbility.thisUnit = this;
            (this as IUnit).ObjectType = UnitFieldObjectType;
            UnitName = $"Базовый Юнит";
        }

        protected virtual void Start()
        {
            if ((this as IUnit).team % 2 == 0)
                UnitSprite.transform.Rotate(Vector3.up, 180);
            teamcolor[0] = this.transform.Find("Circle Red");
            teamcolor[1] = this.transform.Find("Circle Green");
            if (teamcolor[0] != null && teamcolor[0])
            {
                teamcolor[0].gameObject.SetActive((this as IUnit).team % 2 == 0);
                teamcolor[1].gameObject.SetActive((this as IUnit).team % 2 != 0);
            }
        }

        IEnumerator ExecuteMoveByRoute(Vector2Int pos)
        {
            VisualActionStart?.Invoke(this);
            IsBusyByVisual = true;
            float screenMoveSpeed = 5f; //единиц в секунду
            while ((this as IUnit).Route.Count > 1 && (this as IUnit).FieldPosition != pos)
            {
                yield return new WaitForSeconds(0.05f);
                Vector2Int startpos = (this as IUnit).Route[^1];
                Vector2Int targetpos = (this as IUnit).Route[^2];
                (this as IUnit).Route.RemoveAt((this as IUnit).Route.Count - 1);
                // это плохо капец!!!!! логика внутри визуала 
                bool moveSuccess = MoveUnit(targetpos);// отделили логику немного;
                // разделить возможность движения и реальное движение - тогда костыли уберем 
                if (!moveSuccess) break;
                // one step animation
                // вообще костыль: в логике - передвинули объект по полю - здесь - вернули на клетку назад и анимируем 
                // потому что в логике поля зачем-то двигаем изображение 
                this.transform.position = (this as IFieldObject).Field.GetWorldCoord(startpos);
                Vector3 newpos = (this as IUnit).Field.GetWorldCoord(targetpos);
                Vector3 direction = (newpos - this.transform.position).normalized;
                do
                {
                    yield return null;
                    this.transform.position += direction * screenMoveSpeed * Time.deltaTime;
                    if (Vector3.Angle(newpos - this.transform.position, direction) > 10f)
                        this.transform.position = newpos;
                }
                while (this.transform.position != newpos);
            }
            (this as IUnit).Route = null;
            (this as IUnit).weights = null;

            VisualActionEnd?.Invoke(this);
            IsBusyByVisual = false;
        }


        // методы IUnit        
        bool IUnit.IsAvailable() => !IsBusyByVisual;
        void NewRound()
        {
            (this as IUnit).CurrentStats.TurnPoints = (this as IUnit).BaseStats.TurnPoints;
            (this as IUnit).weights = null;
            (this as IUnit).Route = null;
        }

        string IUnit.GetInfo()
        {
            string result =
                $"Name: {UnitName},\n" +
                $"Health {(this as IUnit).CurrentStats.Health} of {(this as IUnit).BaseStats.Health}\n" +
                $"TurnPoints {(this as IUnit).CurrentStats.TurnPoints} of {(this as IUnit).BaseStats.TurnPoints}\n" +
                $"Damage Value {(this as IUnit).AttackAbility.Damage}";
            return result;
        }
        public virtual int CostTile(List<IFieldObject> tileObjects)
        {
            int result = 0;
            foreach (IFieldObject obj in tileObjects)
            {   // выбираем максимум стоимости из значений по умолчанию из интерфейса
                if ((this as IUnit).DefaultCost(obj.ObjectType) > result)
                    result = (this as IUnit).DefaultCost(obj.ObjectType);
            }
            return result;
        }

             
        private bool MoveUnit(Vector2Int targetpos)
        {
            if (!(this as IUnit).Field.IsValidTileCoord(targetpos)) return false;
            int StepCost = (this as IUnit).CostTile((this as IUnit).Field.battlefield[targetpos.x, targetpos.y]);
            if (StepCost > (this as IUnit).CurrentStats.TurnPoints) return false;
            (this as IUnit).CurrentStats.TurnPoints -= StepCost;
            (this as IUnit).Field.MoveObject(this, targetpos);
            return true;
        }

        bool IUnit.MoveByRoute(Vector2Int pos)
        {
            IUnit unit = this as IUnit;
            if (IsBusyByVisual) return false;
            (this as IUnit).weights = (this as IUnit).WeightMapGenerator.GetWeightMap(this);
            if (unit.weights == null) return false;
            /*if (unit.Route == null || unit.Route.Count == 0 ||
                unit.Route[^1] != unit.FieldPosition ||
                unit.Route[0] != pos)*/
            unit.Route = (this as IUnit).WeightMapGenerator.GetPath(this, pos, (this as IUnit).weights);
            if (unit.Route == null || unit.Route.Count < 2) return false;
            if (unit.weights[unit.Route[^2].x, unit.Route[^2].y].turn > 1) return false;
            StartCoroutine(ExecuteMoveByRoute(pos));
            return true;
        }

        public void GetDamage(IcwUnitBaseAttackAbility attack)
        {
            (this as IUnit).Battle.Presenter.ShowText($"{this.name} получает {attack.Damage} урона");
            (this as IUnit).CurrentStats.Health -= attack.Damage;
            if ((this as IUnit).CurrentStats.Health<=0)
            {
                Vector3 pos = this.transform.position;
                Vector2Int fieldPos = (this as IUnit).FieldPosition;
                (this as IUnit).Field.RemoveObject(this);
                GameObject du = Instantiate(DeadUnitPrefab, this.transform.position, Quaternion.identity,  this.transform.parent);
                IFieldObject fo = du.GetComponent<IFieldObject>();
                du.transform.Find("MainSprite").GetComponent<SpriteRenderer>().sprite = this.transform.Find("MainSprite").GetComponent<SpriteRenderer>().sprite; 
                (this as IUnit).Field.AddObject(fo, fieldPos);
                UnitDead?.Invoke(this);
                Destroy(this.gameObject);
            }
        }

        public virtual Vector2Int? DoAttack(Vector2Int pos)
        {
            bool result = (this as IUnit).AttackAbility.IsAttackPossible(pos);
            Vector2Int? targetPos = null;
            if (result)
            {
                targetPos = (this as IUnit).Field.GetFirstObstacleTile((this as IUnit).FieldPosition, pos, false, true);
                if (targetPos != null) pos = targetPos.Value;
                List<IFieldObject> currTile = (this as IUnit).Field.GetObjectsInTile(pos);
                ((IUnit)this).AttackAbility.DoDamage();
                IFieldObject targetObject = currTile.Find(o => o is IDamageable);
                if (targetObject is IDamageable) (targetObject as IDamageable).GetDamage((this as IUnit).AttackAbility);
                (this as IUnit).Route = null;
                (this as IUnit).weights = null;
            }
            return targetPos;
        }
    }
}
