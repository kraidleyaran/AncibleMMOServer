using System;
using AncibleCoreCommon.CommonData.Combat;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class ApplyCombatStatsTraitData : TraitData
    {
        public const string TYPE = "Apply Combat Stats Trait";
        public override string Type => TYPE;

        public CombatStats Stats;
        public bool Permanent;
    }
}