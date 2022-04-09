using System;

namespace AncibleCoreCommon.CommonData.Combat
{
    [Serializable]
    public struct CombatStats
    {
        public int Health;
        public int Strength;
        public int Agility;
        public int Intelligence;
        public int Endurance;
        public int Wisdom;
        public int Dexterity;

        public int PhysicalDefense;
        public int MagicalDefense;

        public int PhysicalCriticalStrike;
        public int MagicalCriticalStrike;

        public int DodgeRating;

        public CombatStats(MonsterCombatStats stats)
        {
            Health = stats.Health;
            PhysicalDefense = stats.PhysicalDefense;
            MagicalDefense = stats.MagicalDefense;
            Strength = 0;
            Agility = 0;
            Intelligence = 0;
            Endurance = 0;
            Wisdom = 0;
            Dexterity = 0;
            PhysicalCriticalStrike = 0;
            MagicalCriticalStrike = 0;
            DodgeRating = 0;
        }


        public static CombatStats operator +(CombatStats stats1, CombatStats stats2)
        {
            return new CombatStats
            {
                Health = stats1.Health + stats2.Health,
                Strength = stats1.Strength + stats2.Strength,
                Agility = stats1.Agility + stats2.Agility,
                Intelligence = stats1.Intelligence + stats2.Intelligence,
                Endurance = stats1.Endurance + stats2.Endurance,
                Wisdom = stats1.Wisdom + stats2.Wisdom,
                Dexterity = stats1.Dexterity + stats2.Dexterity,
                PhysicalDefense = stats1.PhysicalDefense + stats2.PhysicalDefense,
                MagicalDefense = stats1.MagicalDefense + stats2.MagicalDefense,
                PhysicalCriticalStrike = stats1.PhysicalCriticalStrike + stats2.PhysicalCriticalStrike,
                MagicalCriticalStrike = stats1.MagicalCriticalStrike + stats2.MagicalCriticalStrike,
                DodgeRating = stats1.DodgeRating + stats2.DodgeRating
            };
        }

        public static CombatStats operator -(CombatStats stats1, CombatStats stats2)
        {
            return new CombatStats
            {
                Health = stats1.Health - stats2.Health,
                Strength = stats1.Strength - stats2.Strength,
                Agility = stats1.Agility - stats2.Agility,
                Intelligence = stats1.Intelligence - stats2.Intelligence,
                Endurance = stats1.Endurance - stats2.Endurance,
                Wisdom = stats1.Wisdom - stats2.Wisdom,
                Dexterity = stats1.Dexterity - stats2.Dexterity,
                PhysicalDefense = stats1.PhysicalDefense - stats2.PhysicalDefense,
                MagicalDefense = stats1.MagicalDefense - stats2.MagicalDefense,
                PhysicalCriticalStrike = stats1.PhysicalCriticalStrike - stats2.PhysicalCriticalStrike,
                MagicalCriticalStrike = stats1.MagicalCriticalStrike - stats2.MagicalCriticalStrike,
                DodgeRating = stats1.DodgeRating - stats2.DodgeRating
            };
        }
    }
}