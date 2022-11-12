using IcwField;
using UnityEngine;

namespace IcwUnits
{
    public class IcwUnitBaseAttackAbility
    {   // позже придется обернуть в интерфейс , пока ленюсь
        public IUnit thisUnit { get; set; }
        public int Range { get; set; } = 1;
        public int Damage { get; set; } = 20;
        public int AttackCost { get; set; } = 6;

        public bool IsAttackPossible(Vector2Int pos)
        {
            if (thisUnit.CurrentStats.TurnPoints < AttackCost)
            {
                thisUnit.Battle.Presenter.ShowText($"{thisUnit} не хватает очков действия для атаки");
                return false;
            }
            if (thisUnit.Field.Distance(pos, thisUnit.FieldPosition) > Range)
            {
                thisUnit.Battle.Presenter.ShowText($"{thisUnit} цель далеко, не могу атаковать");
                return false;
            }
            return true;
        }
        public bool DoDamage()
        {            
            thisUnit.CurrentStats.TurnPoints -= AttackCost;
            thisUnit.Battle.Presenter.ShowText($"{thisUnit} атакует на {this.Damage}");
            return true;
        }
    }
}
