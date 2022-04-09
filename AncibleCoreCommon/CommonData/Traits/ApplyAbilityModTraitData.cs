using System;
using AncibleCoreCommon.CommonData.Ability;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class ApplyAbilityModTraitData : TraitData
    {
        public const string TYPE = "Apply Ability Mod Trait";
        public override string Type => TYPE;

        public string Ability;
        public string Mod;
        public AbilityModType ModType;
    }
}