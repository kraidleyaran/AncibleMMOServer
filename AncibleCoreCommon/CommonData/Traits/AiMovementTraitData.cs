using System;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class AiMovementTraitData : TraitData
    {
        public const string TYPE = "Ai Movement Trait";
        public override string Type => TYPE;
        public int TicksToMove;
    }
}