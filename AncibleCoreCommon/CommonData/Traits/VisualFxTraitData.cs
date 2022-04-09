using System;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class VisualFxTraitData : TraitData
    {
        public const string TYPE = "Visual FX Trait";
        public override string Type => TYPE;

        public string VisualFx;
    }
}