using System.Collections.Generic;
using UnityEngine;
using IcwField;

namespace IcwUnits
{
    public class IcwUnitStats
    {
        public int TurnPoints = 17;
        public int Health = 100;
        public void SetStats(IcwUnitStats astats)
        {
            TurnPoints = astats.TurnPoints;
            Health = astats.Health;
        }
}
}
