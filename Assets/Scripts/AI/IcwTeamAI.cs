using IcwField;
using IcwBattle;
using IcwUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace IcwTeamAI
{
    class IcwTeamAI : ITeamAI
    {
        Dictionary<IUnit, IUnit> TargetsList = new();
        int currUnitForTurn = 0;

        public List<IUnit> Enemies { get; set; }
        public List<IUnit> TeamMates { get; set; }
        public IField Field { get; set; }
        public IBattle Battle { get; set; }

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
            TeamMates.Sort((o1, o2) => o1.CurrentStats.Health.CompareTo(o2.CurrentStats.Health));

            SelectTargets();
            currUnitForTurn = 0;
        }

        void ITeamAI.DoOneTurn()
        {
            if (currUnitForTurn < 0) currUnitForTurn = 0;
            if (currUnitForTurn >= TeamMates.Count) currUnitForTurn = TeamMates.Count - 1;

            IUnit mate = TeamMates[currUnitForTurn];
            IUnit target = TargetsList[mate];
            if (!target.IsAvailable())
                SelectTargetForMate(mate);
            target = TargetsList[mate];
            bool movementSuccess = true;
            while (mate.CurrentStats.TurnPoints > mate.AttackAbility.AttackCost && movementSuccess)
            {
                if (Field.GetVisibleTiles(mate.FieldPosition, mate.AttackAbility.Range).Contains(target.FieldPosition))
                {
                    mate.DoAttack(target.FieldPosition);
                }
                else
                {
                    Vector2Int minw = mate.WeightMapGenerator.GetMinWeigthTileFromNeigbours(mate.weights, target.FieldPosition, mate.AttackAbility.Range);
                    movementSuccess = mate.MoveByRoute(minw);
                }
            }
            if (currUnitForTurn == TeamMates.Count - 1)
                Battle.DoNextRound();
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
            }
        }
    }
}
