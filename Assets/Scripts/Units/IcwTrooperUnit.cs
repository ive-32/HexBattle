using System.Collections;
using System.Collections.Generic;
using IcwField;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace IcwUnits
{
    class IcwTrooperUnit : IcwBaseUnit, IUnit
    {
        public Sprite bullet;
        public GameObject bullet2;
        public GameObject Light;
        private Color blasterColor;

        protected override void Awake()
        {
            base.Awake();
            (this as IUnit).AttackAbility.Range = 3;
            //StartCoroutine(VisualiseAttack((this as IUnit).Field.GetWorldCoord(new Vector2Int(3,3))));
            blasterColor = Random.ColorHSV(0, 1, 0.7f, 1, 0.4f, 0.6f);
            bullet2.GetComponent<SpriteRenderer>().color = blasterColor;
            Light.GetComponent<Light2D>().color = blasterColor;
            UnitName = $"Штурмовик";
        }

        public override int CostTile(List<IFieldObject> tileObject)
        {
            int cost = base.CostTile(tileObject);
            if (cost == IFieldObject.MaxStepCost) return cost;
            if (tileObject.Exists(o => o.ObjectType.FieldObjectTypeName == "Grass")) cost = 4;
            if (tileObject.Exists(o => o.ObjectType.FieldObjectTypeName == "Gravel")) cost = 1;
            return cost;
        }


        IEnumerator VisualiseAttack(Vector3 targetPosition)
        {
            
            float screenMoveBulletSpeed = 10f; //единиц в секунду
            Vector3 direction = (targetPosition - this.transform.position).normalized;
            float angle = Vector3.SignedAngle(Vector3.left, direction, Vector3.forward);

            /*var newobj = new GameObject();
            var spriteRenderer = newobj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = bullet;
            spriteRenderer.sortingOrder = 5;*/

            (this as IUnit).battle.UnitActionStart(this);
            IsBusyByVisual = true;
            for (int i = 0; i < 3; i++)
            {
                //GameObject bulletObject = Instantiate(newobj, this.transform.position, Quaternion.identity);
                GameObject bulletObject = Instantiate(bullet2, this.transform.position, Quaternion.identity);
                bulletObject.transform.Rotate(Vector3.forward, angle);
                bulletObject.SetActive(true);
                int numiteration = 0;
                Light.SetActive(true);
                while (bulletObject.transform.position != targetPosition && numiteration < 1000)
                {
                    numiteration++;
                    bulletObject.transform.position += direction * screenMoveBulletSpeed * Time.deltaTime;
                    if (Vector3.Angle(direction, (targetPosition - bulletObject.transform.position)) > 10f)
                        bulletObject.transform.position = targetPosition;
                    yield return null;
                    if (numiteration>5) Light.SetActive(false);
                }
                Destroy(bulletObject);
                if (numiteration >= 1000) Debug.LogError($"Cycling in Attack Visualisation for {this}");
                yield return new WaitForSeconds(0.15f);
            }
            IsBusyByVisual = false;
            (this as IUnit).battle.UnitActionComplete(this);
        }

        public override Vector2Int? DoAttack(Vector2Int pos)
        {
            Vector2Int? targettile = base.DoAttack(pos);
            if (targettile != null)
                StartCoroutine(VisualiseAttack((this as IUnit).Field.GetWorldCoord((Vector2Int)targettile)));
            return targettile; 
        }

    }


}
