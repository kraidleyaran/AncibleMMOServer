using System;

namespace AncibleCoreCommon.CommonData.Combat
{
    [Serializable]
    public struct CombatGrowthStats
    {
        public float Health;
        public float Strength;
        public float Agility;
        public float Intelligence;
        public float Endurance;
        public float Wisdom;
        public float Dexterity;
        public float PhysicalDefense;
        public float MagicalDefense;

        public static CombatGrowthStats operator +(CombatGrowthStats stats1, CombatGrowthStats stats2)
        {
            return new CombatGrowthStats
            {
                Health = stats1.Health + stats2.Health,
                Strength = stats1.Strength + stats2.Strength,
                Agility = stats1.Agility + stats2.Agility,
                Intelligence = stats1.Intelligence + stats2.Intelligence,
                Endurance = stats1.Endurance + stats2.Endurance,
                Wisdom = stats1.Wisdom + stats2.Wisdom,
                Dexterity = stats1.Dexterity + stats2.Dexterity,
                PhysicalDefense = stats1.PhysicalDefense + stats2.PhysicalDefense,
                MagicalDefense = stats1.MagicalDefense + stats2.MagicalDefense
            };
        }

        public static CombatGrowthStats operator -(CombatGrowthStats stats1, CombatGrowthStats stats2)
        {
            return new CombatGrowthStats
            {
                Health = stats1.Health - stats2.Health,
                Strength = stats1.Strength - stats2.Strength,
                Agility = stats1.Agility - stats2.Agility,
                Intelligence = stats1.Intelligence - stats2.Intelligence,
                Endurance = stats1.Endurance - stats2.Endurance,
                Wisdom = stats1.Wisdom - stats2.Wisdom,
                Dexterity = stats1.Dexterity - stats2.Dexterity,
                PhysicalDefense = stats1.PhysicalDefense - stats2.PhysicalDefense,
                MagicalDefense = stats1.MagicalDefense - stats2.MagicalDefense
            };
        }

        public static CombatGrowthStats operator -(CombatGrowthStats stats1, CombatStats stats2)
        {
            return new CombatGrowthStats
            {
                Health = stats1.Health - stats2.Health,
                Strength = stats1.Strength - stats2.Strength,
                Agility = stats1.Agility - stats2.Agility,
                Intelligence = stats1.Intelligence - stats2.Intelligence,
                Endurance = stats1.Endurance - stats2.Endurance,
                Wisdom = stats1.Wisdom - stats2.Wisdom,
                Dexterity = stats1.Dexterity - stats2.Dexterity,
                PhysicalDefense = stats1.PhysicalDefense - stats2.PhysicalDefense,
                MagicalDefense = stats1.MagicalDefense - stats2.MagicalDefense
            };
        }


    }
}