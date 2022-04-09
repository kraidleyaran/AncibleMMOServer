using System;

namespace AncibleCoreCommon.CommonData.Items
{
    [Serializable]
    public class ItemData
    {
        public string Name;
        public int MaxStack;
        public int SellValue;
        public virtual ItemType Type => ItemType.General;
    }

    [Serializable]
    public class UseableItemData : ItemData
    {
        public string[] ApplyToUser;
        public int UseOnStack;
        public override ItemType Type => ItemType.Useable;
    }

    [Serializable]
    public class EquippableItemData : ItemData
    {
        public string[] ApplyOnEquip;
        public EquippableSlot Slot;
        public override ItemType Type => ItemType.Equippable;
    }
}