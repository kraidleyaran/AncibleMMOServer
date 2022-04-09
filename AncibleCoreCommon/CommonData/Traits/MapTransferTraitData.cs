using System;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class MapTransferTraitData : TraitData
    {
        public const string TYPE = "Map Transfer Trait";
        public override string Type => TYPE;

        public string Map;
        public Vector2IntData Position;
    }
}