using System;
using AncibleCoreCommon.CommonData.Ability;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class AreaOfEffectTraitData : TraitData
    {
        public const string TYPE = "Area Of Effect Trait";
        public override string Type => TYPE;

        public int Area;
        public string[] ApplyToTargets;
        public AbilityAlignment AlignmentRequirement;
    }
}