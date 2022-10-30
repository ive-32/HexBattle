using IcwField;
using UnityEngine;

namespace IcwUnits
{
    public class IcwUnitBaseAttack
    {   // позже придется обернуть в интерфейс , пока ленюсь
        public IUnit thisUnit { get; set; }
        public float Range { get; set; } = 1.8f;
        public int Damage { get; set; } = 20;
        public int AttackCost { get; set; } = 6;

        public void DoDamage(IUnit anotherUnit)
        {
            if (anotherUnit == null) return;
            if (thisUnit.CurrentStats.TurnPoints < AttackCost)
            {
                thisUnit.battle.Presenter.ShowText($"{thisUnit} не хватает очков действия для атаки");
                return;
            }
            if (Vector2Int.Distance(anotherUnit.FieldPosition, thisUnit.FieldPosition)<Range)
            {
                thisUnit.CurrentStats.TurnPoints -= AttackCost;
                thisUnit.battle.Presenter.ShowText($"{thisUnit} атакует {anotherUnit} на {this.Damage}");
                anotherUnit.GetDamage(this);
            }
            else 
            {
                thisUnit.battle.Presenter.ShowText($"{thisUnit} цель далеко, не могу атаковать");
            }
            thisUnit.Route = null;
            thisUnit.weights = null;
        }

        public void DoDamage(Vector2Int pos)
        {
            IFieldObject obj = thisUnit.Field.battlefield[pos.x, pos.y].Find(o => o.ObjectType == IcwField.IFieldObject.ObjType.Unit);
            if (obj is IUnit) DoDamage(obj as IUnit);
        }

    }
}
