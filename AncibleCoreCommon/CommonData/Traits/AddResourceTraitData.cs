using System;
using AncibleCoreCommon.CommonData.Ability;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class AddResourceTraitData : TraitData
    {
        public const string TYPE = "Add Resource Trait";
        public override string Type => TYPE;

        public ResourceType Resource;
        public int Amount;
    }
}