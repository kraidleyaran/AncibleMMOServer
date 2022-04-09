using System;

namespace AncibleCoreCommon.CommonData.Client
{
    [Serializable]
    public class ClientItemData
    {
        public string Item;
        public int Stack;
        public int Slot;
        public string ItemId;

        public ClientItemData()
        {
            GenerateId();
        }

        public void GenerateId()
        {
            ItemId = Guid.NewGuid().ToString();
        }
    }
}