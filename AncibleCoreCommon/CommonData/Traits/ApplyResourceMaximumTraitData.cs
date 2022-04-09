using System;
using AncibleCoreCommon.CommonData.Ability;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class ApplyResourceMaximumTraitData : TraitData
    {
        public const string TYPE = "Apply Resource Maximum Trait";
        public override string Type => TYPE;

        public ResourceType Resource;
        public int Amount;
        public bool Permanent;
    }
}