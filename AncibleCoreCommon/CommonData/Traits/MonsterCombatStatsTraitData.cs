using System;
using AncibleCoreCommon.CommonData.Combat;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class MonsterCombatStatsTraitData : TraitData
    {
        public const string TYPE = "Monster Combat Stats Trait";
        public override string Type => TYPE;

        public MonsterCombatStats Stats;
    }
}