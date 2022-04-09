using System;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class AiWanderingTraitData : TraitData
    {
        public const string TYPE = "Ai Wandering Trait";
        public override string Type => TYPE;
        public int WanderRange;
        public float ChanceToIdle;
        public IntNumberRange IdleTickRange;
    }
}