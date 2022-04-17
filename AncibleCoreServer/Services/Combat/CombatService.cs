using System.IO;
using AncibleCoreCommon;
using AncibleCoreCommon.CommonData.Combat;
using AncibleCoreCommon.CommonData.Traits;

namespace AncibleCoreServer.Services.Combat
{
    public class CombatService : WorldService
    {
        public static int ManaRegenTicks => _instance._settings.ManaRegenTick;
        public static int FocusRegenTicks => _instance._settings.FocusRegenTick;
        public static int FocusPerRegen => _instance._settings.FocusPerRegen;
        public static float ChanceToWakeFromDamage => _instance._settings.ChanceToWakeFromDamage;
        public static CombatSettings CombatSettings => _instance._settings;

        private static CombatService _instance = null;

        private string _combatSettingsPath = string.Empty;

        private CombatSettings _settings = new CombatSettings();

        public CombatService(string combatSettingsPath)
        {
            _combatSettingsPath = combatSettingsPath;
        }

        public override void Start()
        {
            if (_instance == null)
            {
                _instance = this;
                if (File.Exists(_combatSettingsPath))
                {
                    var json = File.ReadAllText(_combatSettingsPath);
                    var combatSettings = AncibleUtils.FromJson<CombatSettings>(json);
                    if (combatSettings != null)
                    {
                        _settings = combatSettings;
                        Log("Combat settings succesfully loaded!");
                    }
                }
                base.Start();
            }
            
        }

        public static int CalculateBonusDamage(DamageType type, CombatStats totalStats)
        {
            var damage = 0;
            switch (type)
            {
                case DamageType.Physical:

                    var strengthDamage =(int)( _instance._settings.DamagePerStrength * totalStats.Strength);
                    var agilityDamage = (int) (_instance._settings.DamagePerAgility * totalStats.Agility);
                    damage = strengthDamage + agilityDamage;
                    break;
                case DamageType.Magical:
                    var intelligenceDamage = (int)(_instance._settings.DamagePerIntelligence * totalStats.Intelligence);
                    damage = intelligenceDamage;
                    break;
            }

            return damage;
        }

        public static int CalculateCritDamage(DamageType type, CombatStats totalStats, int damage)
        {
            var returnDamage = damage;
            var dexCrit = (int)(_instance._settings.ChanceToCriticallyStrikePerDexterity * totalStats.Dexterity);
            switch (type)
            {
                case DamageType.Physical:
                    var physicalCritRating = totalStats.PhysicalCriticalStrike;
                    var agilityCrit = (int)(_instance._settings.CriticalStrikePerAgility * totalStats.Agility);
                    
                    physicalCritRating += agilityCrit + dexCrit;
                    var critChance = (float)physicalCritRating / _instance._settings.MaxCriticalStrikeRating;
                    if (critChance > _instance._settings.MaxCriticalStrike)
                    {
                        critChance = _instance._settings.MaxCriticalStrike;
                    }

                    var crit = RNGService.Roll(critChance);
                    if (crit)
                    {
                        returnDamage = (int)(returnDamage * _instance._settings.CritMultiplier);
                    }
                    break;
                case DamageType.Magical:
                    var magicalCritRating = totalStats.MagicalCriticalStrike;
                    var intelligenceCrit = (int)(_instance._settings.CriticalStrikePerIntelligence * totalStats.Intelligence);
                    magicalCritRating += intelligenceCrit + dexCrit;
                    var magicalCritChance = (float) magicalCritRating / _instance._settings.MaxCriticalStrikeRating;
                    if (magicalCritChance > _instance._settings.MaxCriticalStrike)
                    {
                        magicalCritChance = _instance._settings.MaxCriticalStrike;
                    }

                    var magicCrit = RNGService.Roll(magicalCritChance);
                    if (magicCrit)
                    {
                        returnDamage = (int)(returnDamage * _instance._settings.CritMultiplier);
                    }
                    break;
            }

            return returnDamage;
        }

        public static int CalculateResistedDamage(DamageType type, CombatStats totalStats, int damage)
        {
            
            var returnDamage = 0;
            if (damage > 0)
            {
                switch (type)
                {
                    case DamageType.Physical:
                        var strengthDefense = (int)(_instance._settings.DefensePerStrength * totalStats.Strength);
                        var physicalEnduranceDefense = (int)(_instance._settings.PhysicalDefensePerEndurance * totalStats.Endurance);
                        var physicalDefense = totalStats.PhysicalDefense + strengthDefense + physicalEnduranceDefense;
                        if (physicalDefense > damage)
                        {
                            var requiredTotalDefense = damage * _instance._settings.DefenseFallOffMultiplier;
                            var remainingPercent = 1f - _instance._settings.DefenseFallOff;
                            var additionalPercent = (physicalDefense / requiredTotalDefense) * remainingPercent;
                            if (additionalPercent >= remainingPercent)
                            {
                                returnDamage = damage;
                            }
                            else
                            {
                                returnDamage = (int)((additionalPercent + remainingPercent) * damage);
                            }
                        }
                        else
                        {
                            returnDamage = (int)((float)physicalDefense / damage * _instance._settings.DefenseFallOff);
                        }
                        break;
                    case DamageType.Magical:
                        var wisdomDefense = (int)(_instance._settings.MagicalDefensePerWisdom * totalStats.Wisdom);
                        var magicalEnduranceDefense = (int)(_instance._settings.MagicalDefensePerEndurance * totalStats.Endurance);
                        var magicalDefense = totalStats.MagicalDefense + wisdomDefense + magicalEnduranceDefense;
                        if (magicalDefense > returnDamage)
                        {
                            
                            var requiredTotalDefense = damage * _instance._settings.DefenseFallOffMultiplier;
                            var remainingPerecent = 1f - _instance._settings.DefenseFallOff;
                            var additionalPerecent = (magicalDefense / requiredTotalDefense) * remainingPerecent;
                            if (additionalPerecent >= remainingPerecent)
                            {
                                returnDamage = damage;
                            }
                            else
                            {
                                returnDamage = (int)((additionalPerecent + remainingPerecent) * damage);
                            }
                        }
                        else
                        {
                            returnDamage = (int)((float)magicalDefense / damage * _instance._settings.DefenseFallOff);
                        }
                        break;
                }
                
            }

            return returnDamage;
        }

        public static int CalculateRegenMana(CombatStats totalStats)
        {
            return (int) (_instance._settings.ManaRegenPerWisdom * totalStats.Wisdom);
        }

        public static int CalculateIncomingSpirit(CombatStats totalStats)
        {
            return (int) (_instance._settings.SpiritIncomingRegenPerWisdom * totalStats.Wisdom);
        }

        public static int CalculateOutgoingSpirit(CombatStats totalStats)
        {
            return (int) (_instance._settings.SpiritOutgoingRegenPerWisdom * totalStats.Wisdom);
        }

        public static int CalculateFocusCostReduction(CombatStats totalStats)
        {
            return (int) (_instance._settings.FocusCostPerWisdom * totalStats.Wisdom);
        }

        public static int CalculateAggroPerHeal(int heal)
        {
            var aggro = (int)(heal * _instance._settings.AggroPerHeal);
            if (aggro <= 0)
            {
                aggro = 1;
            }
            return aggro;
        }

        public static int CalculateAggroPerDamage(int damage)
        {
            var aggro = (int)(damage * _instance._settings.AggroPerDamage);
            if (aggro <= 0)
            {
                aggro = 1;
            }
            return aggro;
        }

        public static int CalculateHealBonus(CombatStats totalStats, DamageType type)
        {
            switch (type)
            {
                case DamageType.Physical:
                    return (int)(totalStats.Wisdom * _instance._settings.PhysicalHealPerWisdom);
                case DamageType.Magical:
                    return (int)(totalStats.Intelligence * _instance._settings.MagicalHealPerIntelligence + totalStats.Wisdom * _instance._settings.MagicalHealPerWisdom);
            }

            return 0;
        }

        public static int GetAggroLossFromDistance(float distance)
        {
            return (int) (distance * _instance._settings.AggroLossPerDistance);
        }

        public static int GetAggroFromStatusEffect()
        {
            return (int)_instance._settings.AggroPerStatusEffect;
        }

        public static int CalculateHealthRegen(CombatStats stats)
        {
            return (int) (stats.Endurance * CombatSettings.HealthRegenPerEndurance) + (CombatSettings.BaseHealthRegen);
        }

        public static bool CalculateDodage(CombatStats stats)
        {
            var dodge = stats.DodgeRating + (int)(stats.Dexterity * CombatSettings.DodgeRatingPerDexterity);
            if (dodge > CombatSettings.MaxDodgeRating)
            {
                dodge = CombatSettings.MaxDodgeRating;
            }

            var dodgeChance = (float)dodge / CombatSettings.MaxDodgeRating;
            if (dodgeChance > CombatSettings.MaxDodgeChance)
            {
                dodgeChance = CombatSettings.MaxDodgeChance;
            }
            return RNGService.Roll(dodgeChance);
        }
    }
}