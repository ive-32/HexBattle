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
    class IcwTeamAI : ITeamAI
    {
        Dictionary<IUnit, IUnit> TargetsList = new();
        int currUnitForTurn = 0;
        bool DebugDepth = false;
        public List<IUnit> Enemies { get; set; } = new();
        public List<IUnit> TeamMates { get; set; } = new();
        public IField Field { get; set; }
        private IBattle battle;
        IBattle ITeamAI.Battle
        {
            get => battle;
            set
            {
                if (battle!=null)
                    battle.OnNewRound -= (this as ITeamAI).DoNewRound;
                battle = value;
                battle.OnNewRound += (this as ITeamAI).DoNewRound;
            }
        }
        public IPresenter Presenter { get; set; }
        public event IUnit.UnitEvent OnAIActionStart;
        public event IUnit.UnitEvent OnAIActionEnd;


        void ITeamAI.DoEquip<T>(List<T> Items)
        {
            throw new NotImplementedException();
        }

        void ITeamAI.AddUnit(IUnit unit, ITeamAI.AITeamType team)
        {
            List<IUnit> list = team == ITeamAI.AITeamType.Mates ? TeamMates : Enemies;
            if (!list.Contains(unit)) list.Add(unit);
            unit.UnitDead += RemoveDead;
        }


        void RemoveDead(object obj)
        {
            if (!(obj is IUnit unit)) return;
            if (DebugDepth) Presenter.ShowText($"AI: {unit.UnitName} был убит. Удаляю из списка");
            TeamMates.Remove(unit);
            Enemies.Remove(unit);
            List<IUnit> keysForRemove = new List<IUnit>();
            foreach(KeyValuePair<IUnit, IUnit> pair in TargetsList)
            {
                if (pair.Value == unit)
                    keysForRemove.Add(pair.Key);
            }
            foreach(IUnit keytoremove in keysForRemove)
                TargetsList.Remove(keytoremove);
        }

        void ITeamAI.DoNewRound() // Следующий раунд
        {
            if (Enemies.Count == 0) return;
            if (TeamMates.Count == 0) return;


            // выбираем цель идем ее убивать

            // сортируем своих по здоровью - самый здоровый первый выбирает цель 
            TeamMates.Sort((o1, o2) => o2.CurrentStats.Health.CompareTo(o1.CurrentStats.Health));
            if (DebugDepth) Presenter.ShowText($"AI: Мы поставили вперед здоровяка с {TeamMates[0].CurrentStats.Health} HP");
            SelectTargets();
            currUnitForTurn = 0;
        }

        void OnVisualEnd(object obj)
        {
            if (!(obj is IUnit mate)) return;
            if (DebugDepth) Presenter.ShowText($"AI: {mate.UnitName} закончил действие");
            if (!DoOneAction(mate)) 
                EndTurn(mate);
        }

        void EndTurn(IUnit mate)
        {
            if (mate != null)
                mate.VisualActionEnd -= OnVisualEnd;
            currUnitForTurn++;
            
            Presenter.ShowText($"AI: ОД закончились. Передаю ход игроку");
            OnAIActionEnd?.Invoke(this);
            battle.DoNextTurn(this);
        }

        bool DoOneAction(IUnit mate)
        {
            Presenter.ShowText($"AI: Думает {mate.UnitName}");
            if (Enemies.Count == 0) return false;

            if (!TargetsList.ContainsKey(mate)) // || TargetsList[mate])
            {
                if (DebugDepth) Presenter.ShowText($"AI: {mate.UnitName} выбирает новую цель");
                SelectTargetForMate(mate);
            }
            if (!TargetsList.ContainsKey(mate)) return false;

            IUnit target = TargetsList[mate];
            if (DebugDepth) Presenter.ShowText($"AI: {mate.UnitName} будет нападать на {target.UnitName}");
            if (mate.CurrentStats.TurnPoints > 0)//; mate.AttackAbility.AttackCost )
            {
                if (DebugDepth) Presenter.ShowText($"AI: {mate.UnitName} AP {mate.CurrentStats.TurnPoints}");
                if (DebugDepth) Presenter.ShowText($"AI: до цели {Field.Distance(mate.FieldPosition, target.FieldPosition)}");
                // плохо по два раза GetFirstObstacleTile
                Vector2Int? vv = Field.GetFirstObstacleTile(mate.FieldPosition, target.FieldPosition, false, true);
                if (vv.HasValue && vv != target.FieldPosition)
                    if (DebugDepth) Presenter.ShowText($"AI: цель перекрыта {vv}");

                if (Field.GetVisibleTiles(mate.FieldPosition, mate.AttackAbility.Range).Contains(target.FieldPosition))
                {
                    if (DebugDepth) Presenter.ShowText($"AI: атакую {target.UnitName}");
                    Vector2Int? attackres = mate.DoAttack(target.FieldPosition);
                    if (attackres.HasValue && attackres == target.FieldPosition)
                        return true;
                }
                else
                {
                    mate.weights = mate.WeightMapGenerator.GetWeightMap(mate);
                    Vector2Int minw = mate.WeightMapGenerator.GetMinWeigthTileFromNeigbours(mate.weights, target.FieldPosition, mate.AttackAbility.Range);

                    if (DebugDepth) Presenter.ShowText($"AI: {mate.UnitName}. Цель стоит {target.FieldPosition} я иду в {minw}");
                    if (mate.MoveByRoute(minw))
                        return true;
                }
            }
            return false;
        }

        void ITeamAI.DoOneTurn()
        {
            OnAIActionStart?.Invoke(this);
            if (TeamMates.Count == 0) 
                EndTurn(null);
            if (Enemies.Count == 0)
                EndTurn(null);

            if (currUnitForTurn < 0) currUnitForTurn = 0;
            if (currUnitForTurn >= TeamMates.Count) currUnitForTurn = TeamMates.Count - 1;
            IUnit mate = TeamMates[currUnitForTurn];
            mate.VisualActionEnd += OnVisualEnd;
            if (!DoOneAction(mate))
                EndTurn(mate);
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
        void CalcWeightForTargetSelect(IUnit mate)
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

        void CalcWeightForTargetSelect()
        {
            // для всех юнитов из команды считаем веса
            foreach (IUnit mate in TeamMates)
                CalcWeightForTargetSelect(mate);
        }

        void SelectTargetForMate(IUnit mate)
        {
            CalcWeightForTargetSelect(); // считаем все карты весов 
            Vector2Int matepos = mate.FieldPosition;
            // ищем цель в пределах хода
            
            IEnumerable<IUnit> targets =
                from e in Enemies
                where mate.weights[e.FieldPosition.x, e.FieldPosition.y].turn == 1
                orderby
                    mate.weights[e.FieldPosition.x, e.FieldPosition.y].cost,
                    e.CurrentStats.Health
                select e;

            if (targets !=null && targets.Any())
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
                    if (DebugDepth) Presenter.ShowText($"AI: {mate.UnitName} будет атаковать {TargetsList[mate].UnitName}");
                else
                    if (DebugDepth) Presenter.ShowText($"AI: {mate.UnitName} не нашел кого атаковать");
            }
        }

        
    }
}
