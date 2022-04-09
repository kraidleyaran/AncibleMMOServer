using System;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class AiAggroTraitData : TraitData
    {
        public const string TYPE = "Ai Aggro Trait";
        public override string Type => TYPE;

        public int AggroRange;
        public int AggroCheckTicks;
        public bool HealOnAggroDrop;
    }
}