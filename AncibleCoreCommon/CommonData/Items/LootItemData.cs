using System;

namespace AncibleCoreCommon.CommonData.Items
{
    [Serializable]
    public class LootItemData
    {
        public string Item;
        public IntNumberRange Stack;
        public float ChanceToDrop;
    }
}