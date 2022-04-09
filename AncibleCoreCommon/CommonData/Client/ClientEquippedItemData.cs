using System;
using AncibleCoreCommon.CommonData.Items;

namespace AncibleCoreCommon.CommonData.Client
{
    [Serializable]
    public class ClientEquippedItemData
    {
        public string Item { get; set; }
        public EquippableSlot Slot { get; set; }
        public string ItemId { get; set; }
    }
}