using System;
using System.Collections.Generic;
using System.Linq;
using AncibleCoreCommon;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Items;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class ShopTrait : ObjectTrait
    {
        private ShopItemData[] _shopItems = new ShopItemData[0];

        public ShopTrait(TraitData data) : base(data)
        {
            if (data is ShopTraitData shopData)
            {
                _shopItems = shopData.ShopItems;
                for (var i = 0; i < _shopItems.Length; i++)
                {
                    _shopItems[i].ShopId = Guid.NewGuid().ToString();
                }
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            SubscribeToMessages();
            if (!_parent.Interactions.Contains(InteractionType.Shop))
            {
                _parent.Interactions.Add(InteractionType.Shop);
            }
        }

        private void SubscribeToMessages()
        {
            _parent.SubscribeWithFilter<QueryIsShopMessage>(QueryIsShop, _instanceId);
            _parent.SubscribeWithFilter<QueryShopItemByIdMessage>(QueryShopItemById, _instanceId);
            _parent.SubscribeWithFilter<InteractWithObjectMessage>(InteractWithObject, _instanceId);
        }

        private void InteractWithObject(InteractWithObjectMessage msg)
        {
            if (msg.Type == InteractionType.Shop)
            {
                this.SendMessageTo(new ClientShowShopMessage { ShopItems = _shopItems, ObjectId = _parent.Id }, msg.Owner);
            }
        }

        private void QueryIsShop(QueryIsShopMessage msg)
        {
            msg.DoAfter.Invoke();
        }

        private void QueryShopItemById(QueryShopItemByIdMessage msg)
        {
            var shopItem = _shopItems.FirstOrDefault(i => i.ShopId == msg.ShopId);
            if (shopItem != null)
            {
                msg.DoAfter.Invoke(shopItem);
            }
        }
    }
}