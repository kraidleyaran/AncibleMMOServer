using System;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class CheckpointTraitData : TraitData
    {
        public const string TYPE = "Checkpoint Trait";
        public override string Type => TYPE;

        public Vector2IntData RelativePosition;
    }
}