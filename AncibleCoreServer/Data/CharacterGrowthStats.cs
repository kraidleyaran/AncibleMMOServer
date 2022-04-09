using System;
using AncibleCoreCommon.CommonData.Combat;

namespace AncibleCoreServer.Data
{
    [Serializable]
    public class CharacterGrowthStats : WorldData
    {
        public const string TABLE = "CharacterGrowth";
        public int CharacterId { get; set; }
        public float Health { get; set; }
        public float Strength { get; set; }
        public float Agility { get; set; }
        public float Intelligence { get; set; }
        public float Endurance { get; set; }
        public float Wisdom { get; set; }
        public float Dexterity { get; set; }
        public float PhysicalDefense { get; set; }
        public float MagicalDefense { get; set; }

        public CharacterGrowthStats()
        {

        }

        public CharacterGrowthStats(CombatGrowthStats stats)
        {
            FromGrowth(stats);
        }

        public void FromGrowth(CombatGrowthStats stats)
        {
            Health = stats.Health;
            Strength = stats.Strength;
            Agility = stats.Agility;
            Intelligence = stats.Intelligence;
            Endurance = stats.Endurance;
            Wisdom = stats.Wisdom;
            Dexterity = stats.Dexterity;
            PhysicalDefense = stats.PhysicalDefense;
            MagicalDefense = stats.MagicalDefense;
        }
    }
}