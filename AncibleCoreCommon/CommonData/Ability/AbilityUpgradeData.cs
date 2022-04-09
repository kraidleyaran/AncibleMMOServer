using System;

namespace AncibleCoreCommon.CommonData.Ability
{
    [Serializable]
    public class AbilityUpgradeData
    {
        public ResourceCost[] Resources;
        public int Cooldown;
        public int CastTime;
        public int Range;
        public string[] ApplyToOwner;
        public string[] ApplyToTarget;
    }
}