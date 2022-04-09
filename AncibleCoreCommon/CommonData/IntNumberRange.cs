using System;

namespace AncibleCoreCommon.CommonData
{
    [Serializable]
    public class IntNumberRange
    {
        public int Minimum = 0;
        public int Maximum = 0;

        public IntNumberRange()
        {

        }

        public IntNumberRange(int min, int max)
        {

        }

        public int GenerateRandomNumber(Random rng)
        {
            if (Minimum < Maximum)
            {
                return rng.Next(Minimum, Maximum + 1);
            }
            return Maximum;
        }

        public int GenerateRandomNumberWithBonus(Random rng, int bonus)
        {
            var bonusMinimum = Minimum + bonus;
            var bonusMaximum = Maximum + bonus;
            if (bonusMinimum < bonusMaximum)
            {
                return rng.Next(bonusMinimum, bonusMaximum + 1);
            }

            return bonusMaximum;
        }

        public IntNumberRange Clone()
        {
            return new IntNumberRange { Minimum = Minimum, Maximum = Maximum };
        }

        public static IntNumberRange operator +(IntNumberRange range1, IntNumberRange range2)
        {
            return new IntNumberRange{Minimum = range1.Minimum + range2.Minimum, Maximum = range1.Maximum + range2.Maximum};
        }

        public static IntNumberRange operator -(IntNumberRange range1, IntNumberRange range2)
        {
            return new IntNumberRange { Minimum = range1.Minimum - range2.Minimum, Maximum = range1.Maximum - range2.Maximum };
        }

        public override string ToString()
        {
            if (Maximum > Minimum) 
            {
                return $"{Minimum} - {Maximum}";
            }

            return $"{Maximum}";
        }

        
    }
}