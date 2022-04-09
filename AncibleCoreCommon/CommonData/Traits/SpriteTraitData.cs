using System;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class SpriteTraitData : TraitData
    {
        public const string TYPE = "Sprite Trait";
        public override string Type => TYPE;

        public string Sprite;
    }
}