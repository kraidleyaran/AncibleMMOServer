using System;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class CastingTraitData : TraitData
    {
        public const string TYPE = "Casting Trait";
        public override string Type => TYPE;
    }
}