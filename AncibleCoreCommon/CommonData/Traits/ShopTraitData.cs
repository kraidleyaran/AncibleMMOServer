using System;
using AncibleCoreCommon.CommonData.Items;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class ShopTraitData : TraitData
    {
        public const string TYPE = "Shop Trait";
        public override string Type => TYPE;

        public ShopItemData[] ShopItems;
    }
}