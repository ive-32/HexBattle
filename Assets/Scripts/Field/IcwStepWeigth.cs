using System.Collections.Generic;
using System;

namespace IcwField
{
    public class IcwStepWeigth : IComparable
    {
        public static int DefaultMaxValue = 100000;
        public int turn = DefaultMaxValue;
        public int cost = DefaultMaxValue;
        public IcwStepWeigth() { }

        public IcwStepWeigth(int aturn, int acost) 
        {
            turn = aturn;
            cost = acost;
        }
        
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            //IcwStepWeigth otherWeigth = obj as IcwStepWeigth;
            if (obj is IcwStepWeigth otherWeigth)
            {
                if (this.turn < otherWeigth.turn ||
                    (this.turn == otherWeigth.turn && this.cost < otherWeigth.cost)) 
                    return -1;
                if (this.turn == otherWeigth.turn && this.cost == otherWeigth.cost)
                    return 0;
                return 1;
            }
            else
                throw new ArgumentException("Object is not a IcwStepWeight");
        }
    }
}
