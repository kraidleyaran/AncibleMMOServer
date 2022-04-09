using System;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class WeaponDamageTraitData : TraitData
    {
        public const string TYPE = "Weapon Damage Trait";
        public override string Type => TYPE;

        public IntNumberRange Damage;
    }
}