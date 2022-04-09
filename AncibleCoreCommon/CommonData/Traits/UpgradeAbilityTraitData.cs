using System;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class UpgradeAbilityTraitData : TraitData
    {
        public const string TYPE = "Upgrade Ability Trait";
        public override string Type => TYPE;

        public string Ability;
    }
}