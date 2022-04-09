using System;
using AncibleCoreCommon.CommonData.Client;
using AncibleCoreCommon.CommonData.Items;

namespace AncibleCoreServer.Data
{
    [Serializable]
    public class CharacterEquippedItem : WorldData
    {
        public const string TABLE = "CharacterEquipment";
        public string Item { get; set; }
        public EquippableSlot Slot { get; set; }
        public string ItemId { get; set; }

        public CharacterEquippedItem()
        {

        }

        public CharacterEquippedItem(ClientEquippedItemData data)
        {
            FromClientData(data);
        }

        public void FromClientData(ClientEquippedItemData data)
        {
            Item = data.Item;
            Slot = data.Slot;
            ItemId = data.ItemId;
        }
    }
}