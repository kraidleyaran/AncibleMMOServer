using System;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class CombatTraitData : TraitData
    {
        public const string TYPE = "Combat Trait";
        public override string Type => TYPE;
        public CombatAlignment Alignment;
    }
}