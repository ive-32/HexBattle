using System.Collections.Generic;
using System.Collections;
using IcwField;
using UnityEngine;

namespace IcwUnits
{
    class IcwEvoqueUnit : IcwBaseUnit, IUnit
    {
        public GameObject mainsprite;

        protected override void Awake()
        {
            base.Awake();
            (this as IUnit).AttackAbility.AttackCost = 3;
            (this as IUnit).AttackAbility.Damage = 10;
            (this as IUnit).BaseStats.TurnPoints = Random.Range(20, 28);
            UnitName = $"Эвок";
        }

        public override int CostTile(List<IFieldObject> tileObject)
        {
            int cost = base.CostTile(tileObject);
            if (cost == IFieldObject.MaxStepCost) return cost;
            if (tileObject.Exists(o => o.ObjectType.FieldObjectTypeName == "Grass")) cost = 2;
            if (tileObject.Exists(o => o.ObjectType.FieldObjectTypeName == "Gravel")) cost = 6;
            return cost;
        }

        IEnumerator VisualiseAttack(Vector3 targetPosition)
        {
            RaiseVisualActionStart(this);
            IsBusyByVisual = true;

            Vector3 direction = (targetPosition - this.transform.position).normalized;
            float screenMoveBulletSpeed = 5f;

            int numiteration = 0;
            while (mainsprite.transform.position != targetPosition && numiteration < 1000)
            {
                numiteration++;
                mainsprite.transform.position += direction * screenMoveBulletSpeed * Time.deltaTime;
                if (Vector3.Angle(direction, (targetPosition - mainsprite.transform.position)) > 10f)
                    mainsprite.transform.position = targetPosition;
                yield return null;
            }
            if (numiteration >= 1000) Debug.LogError($"Cycling in Attack Visualisation for {(this as IUnit).UnitName} \n target {targetPosition}");
            targetPosition = this.transform.position;
            while (mainsprite.transform.position != targetPosition && numiteration < 1000)
            {
                numiteration++;
                mainsprite.transform.position += direction * screenMoveBulletSpeed * Time.deltaTime;
                if (Vector3.Angle(direction, (targetPosition - mainsprite.transform.position)) > 10f)
                    mainsprite.transform.position = targetPosition;
                yield return null;
            }
            if (numiteration >= 1000) Debug.LogError($"Cycling in Attack Visualisation for {this}");
            RaiseVisualActionEnd(this);
            IsBusyByVisual = false;
        }

        public override Vector2Int? DoAttack(Vector2Int pos)
        {
            Vector2Int? targettile = base.DoAttack(pos);
            if (targettile != null)
                StartCoroutine(VisualiseAttack((this as IUnit).Field.GetWorldCoord(targettile.Value)));
            return targettile;
        }
    }
}
