using System;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class DamageTraitData : TraitData
    {
        public const string TYPE = "Damage Trait";
        public override string Type => TYPE;

        public IntNumberRange Amount;
        public DamageType DamageType;
        public float BonusMultiplier;
        public bool UseWeaponDamage;
        public string[] Tags = new string[0];
    }
}