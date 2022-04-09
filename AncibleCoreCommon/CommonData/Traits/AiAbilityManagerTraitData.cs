using System;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class AiAbilityManagerTraitData : TraitData
    {
        public const string TYPE = "Ai Ability Manager Trait";
        public override string Type => TYPE;

        public string[] Abilities;
    }
}