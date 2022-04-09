using System;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class HealTraitData : TraitData
    {
        public const string TYPE = "Heal Trait";
        public override string Type => TYPE;

        public IntNumberRange Amount;
        public DamageType DamageType;
        public bool ApplyBonus;
        public bool Broadcast;
    }
}