using System;

namespace AncibleCoreCommon.CommonData.Items
{
    [Serializable]
    public class LootTableData
    {
        public string Name;
        public string DisplayName;
        public IntNumberRange Gold;
        public IntNumberRange ItemDrops;
        public LootItemData[] Items;
        public string Sprite;
    }
}