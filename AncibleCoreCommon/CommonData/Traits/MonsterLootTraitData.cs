using System;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class MonsterLootTraitData : TraitData
    {
        public const string TYPE = "Monster Loot Trait";
        public override string Type => TYPE;

        public string LootTable;
        public IntNumberRange Experience;
    }
}