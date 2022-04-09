using System;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class DestroyObjectTraitData : TraitData
    {
        public const string TYPE = "Destroy Object Trait";
        public override string Type => TYPE;
    }
}