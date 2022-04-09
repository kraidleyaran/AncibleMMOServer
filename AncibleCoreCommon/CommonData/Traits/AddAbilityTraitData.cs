using System;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class AddAbilityTraitData : TraitData
    {
        public const string TYPE = "Add Ability Trait";
        public override string Type => TYPE;

        public string Ability;
    }
}