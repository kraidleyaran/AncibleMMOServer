using System;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class HealerTraitData : TraitData
    {
        public const string TYPE = "Healer Trait";
        public override string Type => TYPE;

        public string[] ApplyOnInteract;
    }
}