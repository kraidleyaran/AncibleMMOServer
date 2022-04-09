using System;
using AncibleCoreCommon.CommonData.Client;
using AncibleCoreCommon.CommonData.Items;

namespace AncibleCoreServer.Services.Items
{
    public class ItemStack
    {
        public ItemData Item;
        public int Stack;
        public string Id;

        public ItemStack()
        {
            Id = Guid.NewGuid().ToString();
        }

        public ClientLootItemData ToClientLootData()
        {
            return new ClientLootItemData {Item = Item.Name, Stack = Stack, Id = Id};
        }

        public void Destroy()
        {
            Item = null;
            Stack = 0;
            Id = null;
        }
    }


}