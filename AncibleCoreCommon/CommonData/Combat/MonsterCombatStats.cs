using System;

namespace AncibleCoreCommon.CommonData.Combat
{
    [Serializable]
    public class MonsterCombatStats
    {
        public int Health;
        public int PhysicalDefense;
        public int MagicalDefense;

        public static MonsterCombatStats operator +(MonsterCombatStats stats1, MonsterCombatStats stats2)
        {
            return new MonsterCombatStats
            {
                Health = stats1.Health + stats2.Health,
                PhysicalDefense = stats1.PhysicalDefense + stats2.PhysicalDefense,
                MagicalDefense = stats1.MagicalDefense + stats2.MagicalDefense
            };
        }

        public static MonsterCombatStats operator -(MonsterCombatStats stats1, MonsterCombatStats stats2)
        {
            return new MonsterCombatStats
            {
                Health = stats1.Health - stats2.Health,
                PhysicalDefense = stats1.PhysicalDefense - stats2.PhysicalDefense,
                MagicalDefense = stats1.MagicalDefense - stats2.MagicalDefense
            };
        }

        public static MonsterCombatStats operator +(MonsterCombatStats stats1, CombatStats stats2)
        {
            return new MonsterCombatStats
            {
                Health = stats1.Health + stats2.Health,
                PhysicalDefense = stats1.PhysicalDefense + stats2.PhysicalDefense,
                MagicalDefense = stats1.MagicalDefense + stats2.MagicalDefense
            };
        }

        public static MonsterCombatStats operator -(MonsterCombatStats stats1, CombatStats stats2)
        {
            return new MonsterCombatStats
            {
                Health = stats1.Health - stats2.Health,
                PhysicalDefense = stats1.PhysicalDefense - stats2.PhysicalDefense,
                MagicalDefense = stats1.MagicalDefense - stats2.MagicalDefense
            };
        }
    }
}