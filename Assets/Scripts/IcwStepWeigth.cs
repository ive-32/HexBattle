using System.Collections.Generic;

namespace IcwField
{
    public class IcwStepWeigth
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

    }
}
