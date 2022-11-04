using IcwField;
using IcwBattle;
using IcwUnits;
using IcwUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

namespace IcwAI
{
    class IcwTeamAI : MonoBehaviour, ITeamAI
    {
        Dictionary<IUnit, IUnit> TargetsList = new();
        int currUnitForTurn = 0;

        public List<IUnit> Enemies { get; set; } = new();
        public List<IUnit> TeamMates { get; set; } = new();
        public IField Field { get; set; }
        public IBattle Battle { get; set; }
        public IPresenter Presenter { get; set; }

        void ITeamAI.DoEquip<T>(List<T> Items)
        {
            throw new NotImplementedException();
        }

        void ITeamAI.DoNewRound() // Следующий раунд
        {
            if (Enemies.Count == 0) return;
            if (TeamMates.Count == 0) return;

            // выбираем цель идем ее убивать

            // сортируем своих по здоровью - самый здоровый первый выбирает цель 
            TeamMates.Sort((o1, o2) => o2.CurrentStats.Health.CompareTo(o1.CurrentStats.Health));
            Presenter.ShowText($"Мы поставили вперед здоровяка с {TeamMates[0].CurrentStats.Health} HP");
            SelectTargets();
            currUnitForTurn = 0;
        }

        void ITeamAI.DoOneTurn()
        {
            if (currUnitForTurn < 0) currUnitForTurn = 0;
            if (currUnitForTurn >= TeamMates.Count) currUnitForTurn = TeamMates.Count - 1;

            IUnit mate = TeamMates[currUnitForTurn];
            Presenter.ShowText($"Ходит {mate.UnitName}");
            IUnit target = TargetsList[mate];
            if (!target.IsAvailable())
            {
                Presenter.ShowText($"{mate.UnitName} выбирает новую цель");
                SelectTargetForMate(mate);
            }
            target = TargetsList[mate];
            bool movementSuccess = true;
            while (mate.CurrentStats.TurnPoints > mate.AttackAbility.AttackCost && movementSuccess)
            {
                Presenter.ShowText($"{mate.UnitName} AP {mate.CurrentStats.TurnPoints}");
                Presenter.ShowText($"до цели {Field.Distance(mate.FieldPosition , target.FieldPosition)}");
                Vector2Int? vv = Field.GetFirstObstacleTile(mate.FieldPosition, target.FieldPosition);
                if (vv.HasValue && vv!=target.FieldPosition)
                {
                    Presenter.ShowText($"цель перекрыта {vv}");
                }

                if (Field.GetVisibleTiles(mate.FieldPosition, mate.AttackAbility.Range).Contains(target.FieldPosition))
                {
                    Presenter.ShowText($"атакую {target.UnitName}");
                    mate.DoAttack(target.FieldPosition);
                }
                else
                {
                    Presenter.ShowText($"{mate.UnitName} не может атаковать {target.UnitName}");
                    Vector2Int minw = mate.WeightMapGenerator.GetMinWeigthTileFromNeigbours(mate.weights, target.FieldPosition, mate.AttackAbility.Range);
                    Presenter.ShowText($"{mate.UnitName} ходит на {minw}");
                    movementSuccess = mate.MoveByRoute(minw);
                }
            }

            currUnitForTurn++;
            if (currUnitForTurn >= TeamMates.Count)
            {
                Presenter.ShowText($"Перебрали всех юнитов запускаем следующий круг");
                currUnitForTurn = 0;
                Battle.DoNextRound();
            }
            else
                Battle.DoNextTurn();
        }

        int GetEnemyWeight(IUnit enemy)
        {
            int enemyweight = 0;
            // чем здоровее юнит, тем меньше вес
            enemyweight -= enemy.CurrentStats.Health;
            foreach(IUnit mate in TeamMates)
            {
                Vector2Int matepos = mate.FieldPosition;
                // чем дольше до юнита бежать, тем меньше вес
                enemyweight -= mate.weights[matepos.x, matepos.y].turn * (mate.BaseStats.TurnPoints / mate.AttackAbility.AttackCost) * mate.AttackAbility.Damage;
                enemyweight -= mate.weights[matepos.x, matepos.y].cost;
                if (TargetsList.ContainsValue(enemy)) enemyweight += mate.BaseStats.TurnPoints / mate.AttackAbility.AttackCost * mate.AttackAbility.Damage;
            }
            return enemyweight;
        }

        void CalcWeightForTargetSelect()
        {
            // для всех юнитов из команды считаем веса
            foreach (IUnit mate in TeamMates)
            {
                mate.weights = mate.WeightMapGenerator.GetWeightMap(mate);
                // заполняем в карте весов клетки с врагами минимальным весом клетки откуда его можно атаковать
                foreach (IUnit enemy in Enemies)
                {
                    Vector2Int minw = mate.WeightMapGenerator.GetMinWeigthTileFromNeigbours(mate.weights, enemy.FieldPosition, mate.AttackAbility.Range);
                    mate.weights[enemy.FieldPosition.x, enemy.FieldPosition.y] =
                        mate.weights[minw.x, minw.y];
                }
            }
        }

        void SelectTargetForMate(IUnit mate)
        {
            Vector2Int matepos = mate.FieldPosition;
            // ищем цель в пределах хода
            IEnumerable<IUnit> targets =
                from e in Enemies
                where mate.weights[e.FieldPosition.x, e.FieldPosition.y].turn == 1
                orderby
                    mate.weights[e.FieldPosition.x, e.FieldPosition.y].cost,
                    e.CurrentStats.Health
                select e;

            if (targets.Count() > 0)
            {   // есть цель в пределах хода берем ее
                TargetsList.Add(mate, targets.First());
                return;
            }

            // в пределах хода не нашли, ищем еще
            targets =
                from e in Enemies
                orderby
                    GetEnemyWeight(e)
                select e;
            if (targets.Count() > 0)
            {   // есть цель берем ее
                TargetsList.Add(mate, targets.First());
                return;
            }
            // ничего не вышло найти просто добавляем первую из списка
            TargetsList.Add(mate, Enemies[0]);
        }

        void SelectTargets()
        {
            // чистим список целей
            TargetsList.Clear();
            CalcWeightForTargetSelect();
            // распределяем цели по своим юнитам
            foreach (IUnit mate in TeamMates)
            {
                SelectTargetForMate(mate);
                if (TargetsList.ContainsKey(mate))
                    Presenter.ShowText($"{mate.UnitName} будет атаковать {TargetsList[mate].UnitName}");
                else
                    Presenter.ShowText($"{mate.UnitName} не нашел кого атаковать");

            }
        }

        /*private void Awake()
        {
            IPresenter tmpp;
            this.TryGetComponent<IPresenter>(out tmpp);
            if (tmpp == null)
                Destroy(this.gameObject);
            Presenter = tmpp;

            IField tmpfield;
            this.TryGetComponent<IField>(out tmpfield);
            if (tmpfield == null)
            {
                Presenter.ShowText("Field object not founded");
                Destroy(this.gameObject);
            }
            Field = tmpfield;

            IBattle tmpbattle;
            this.TryGetComponent<IBattle>(out tmpbattle);
            if (tmpbattle == null)
            {
                Presenter.ShowText("Battle object not founded");
                Destroy(this.gameObject);
            }
            Battle = tmpbattle;
        }*/
    }
}
