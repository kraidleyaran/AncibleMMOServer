using System;

namespace AncibleCoreCommon.CommonData.Ability
{
    [Serializable]
    public class AbilityData
    {
        public string Name;
        public string PositionName;
        public ResourceCost[] Resources;
        public int Cooldown;
        public int CastTime;
        public string[] ApplyToOwner;
        public string[] ApplyToTarget;
        public int Range;
        public AbilityUpgradeData[] Upgrades;
        public TargetType TargetType;
        public AbilityAlignment TargetAlignment;


    }
}