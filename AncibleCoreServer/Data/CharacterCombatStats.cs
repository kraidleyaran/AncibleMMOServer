using System;
using AncibleCoreCommon.CommonData.Combat;

namespace AncibleCoreServer.Data
{
    [Serializable]
    public class CharacterCombatStats : WorldData
    {
        public const string TABLE = "CharacterCombatStats";
        public int CharacterId { get; set; }
        public int CurrentHealth { get; set; }
        public int Health { get; set; }
        public int Strength { get; set; }
        public int Agility { get; set; }
        public int Intelligence { get; set; }
        public int Endurance { get; set; }
        public int Wisdom { get; set; }
        public int Dexterity{ get; set; }

        public int PhysicalDefense { get; set; }
        public int MagicalDefense { get; set; }

        public int PhysicalCrit { get; set; }
        public int MagicalCrit { get; set; }

        public int DodgeRating { get; set; }

        public CharacterCombatStats()
        {

        }

        public CharacterCombatStats(CombatStats stats)
        {
            FromCombatStats(stats, stats.Health);
        }

        public void FromCombatStats(CombatStats stats, int characterHealth)
        {
            Health = stats.Health;
            CurrentHealth = characterHealth;
            Strength = stats.Strength;
            Agility = stats.Agility;
            Intelligence = stats.Intelligence;
            Endurance = stats.Endurance;
            Wisdom = stats.Wisdom;
            Dexterity = stats.Dexterity;
            PhysicalDefense = stats.PhysicalDefense;
            MagicalDefense = stats.MagicalDefense;
            PhysicalCrit = stats.PhysicalCriticalStrike;
            MagicalCrit = stats.MagicalCriticalStrike;
            DodgeRating = stats.DodgeRating;
        }

    }
}