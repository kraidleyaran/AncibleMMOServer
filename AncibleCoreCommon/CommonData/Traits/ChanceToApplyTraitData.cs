using System;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class ChanceToApplyTraitData : TraitData
    {
        public const string TYPE = "Chance To Apply Trait";
        public override string Type => TYPE;

        public float ChanceToApply;
        public string[] ApplyOnChance;
        public string[] Tags;
    }
}