using System;
using AncibleCoreCommon.CommonData.Combat;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class StatusEffectTraitData : TraitData
    {
        public const string TYPE = "Status Effect Trait";
        public override string Type => TYPE;

        public StatusEffectType EffectType;
        public int Length;
    }
}