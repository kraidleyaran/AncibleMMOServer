using System;
using AncibleCoreCommon.CommonData.Client;

namespace AncibleCoreServer.Data
{
    [Serializable]
    public class WorldItem : WorldData
    {
        public const string INVENTORY_TABLE = "CharacterInventory";
        public const string STASH_TABLE = "CharacterStash";
        public string Item { get; set; }
        public int Stack { get; set; }
        public int Slot { get; set; }
        public string ItemId { get; set; }

        public WorldItem()
        {

        }

        public WorldItem(ClientItemData data)
        {
            FromData(data);
        }

        public void FromData(ClientItemData data)
        {
            Item = data.Item;
            Stack = data.Stack;
            Slot = data.Slot;
            ItemId = data.ItemId;
        }
    }
}