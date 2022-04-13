using System;

namespace AncibleCoreCommon.CommonData.Combat
{
    [Serializable]
    public class CombatSettings
    {
        public float DefensePerStrength { get; set; }
        public float DamagePerStrength { get; set; }
        public float CriticalStrikePerAgility { get; set; }
        public float DamagePerAgility { get; set; }
        public float DamagePerIntelligence { get; set; }
        public float CriticalStrikePerIntelligence { get; set; }
        public float MagicalHealPerIntelligence { get; set; }
        public float PhysicalDefensePerEndurance { get; set; }
        public float MagicalDefensePerEndurance { get; set; }
        public float MagicalDefensePerWisdom { get; set; }
        public float PhysicalHealPerWisdom { get; set; }
        public float MagicalHealPerWisdom { get; set; }
        public float DodgeRatingPerDexterity { get; set; }
        public float ChanceToCriticallyStrikePerDexterity { get; set; }

        public int MaxCriticalStrikeRating { get; set; }
        public float MaxCriticalStrike { get; set; }
        public int MaxDodgeRating { get; set; }
        public float MaxDodgeChance { get; set; }
        public float CritMultiplier { get; set; }
        public float DefenseFallOff { get; set; }
        public float DefenseFallOffMultiplier { get; set; }

        public float ManaRegenPerWisdom { get; set; }
        public int ManaRegenTick { get; set; }
        public float SpiritIncomingRegenPerWisdom { get; set; }
        public float SpiritOutgoingRegenPerWisdom { get; set; }
        public float FocusCostPerWisdom { get; set; }
        public int FocusRegenTick { get; set; }
        public int FocusPerRegen { get; set; }
        public int SpiritDegenTick { get; set; }

        public float AggroPerStatusEffect { get; set; }
        public float AggroPerHeal { get; set; }
        public float AggroPerDamage { get; set; }
        public float AggroLossPerDistance { get; set; }

        public float ChanceToWakeFromDamage { get; set; }

        public float HealthRegenPerEndurance { get; set; }
        public int HealthRegenTick { get; set; }
        public int BaseHealthRegen { get; set; }
        public int HealthRegenCooldown { get; set; }
    }
}