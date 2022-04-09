using System;
using AncibleCoreCommon.CommonData.Ability;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class RemoveResourceTraitData : TraitData
    {
        public const string TYPE = "Remove Resource Trait";
        public override string Type => TYPE;

        public ResourceType Resource;
        public int Amount;
    }
}