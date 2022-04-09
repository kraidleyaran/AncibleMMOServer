using System.Collections.Generic;
using System.Linq;
using AncibleCoreCommon;
using AncibleCoreCommon.CommonData.Client;
using AncibleCoreCommon.CommonData.Items;
using AncibleCoreCommon.CommonData.PlayerEvent;
using AncibleCoreServer.Data;
using AncibleCoreServer.Services.Items;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class PlayerEquipmentTrait : ObjectTrait
    {
        private const string NAME = "Player Equipment Trait";

        private Dictionary<EquippableSlot, EquippedItem> _equipment = new Dictionary<EquippableSlot, EquippedItem>
        {
            {EquippableSlot.Helm, null},
            {EquippableSlot.Chest, null},
            {EquippableSlot.Gloves, null},
            {EquippableSlot.Feet, null},
            {EquippableSlot.MainHand, null},
            {EquippableSlot.OffHand, null},
            {EquippableSlot.Ring, null},
            {EquippableSlot.Neck, null},
            {EquippableSlot.Charm, null},
            {EquippableSlot.Relic, null}
        };

        private string _playerId = string.Empty;

        public PlayerEquipmentTrait(string playerId)
        {
            Name = NAME;
            _playerId = playerId;
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            SubscirbeToMessages();
        }

        private void SubscirbeToMessages()
        {
            this.SubscribeWithFilter<ClientUnEquipItemFromSlotMessage>(ClientUnEquipItemFromSlot, _playerId);

            _parent.SubscribeWithFilter<EquipItemMessage>(EquipItem, _instanceId);
            _parent.SubscribeWithFilter<QueryEquipmentMessage>(QueryEquipment, _instanceId);
            _parent.SubscribeWithFilter<SetCharacterEquipmentMessage>(SetCharacterEquipment, _instanceId);
        }

        private void EquipItem(EquipItemMessage msg)
        {
            var item = ItemService.GetItemByName(msg.Item.Item);
            if (item is EquippableItemData equippableItem)
            {
                if (_equipment[equippableItem.Slot] != null)
                {
                    var returnItem = ItemService.GetItemByName(_equipment[equippableItem.Slot].Data.Item);
                    if (returnItem != null)
                    {
                        this.SendMessageTo(new AddItemByIdMessage{ItemId = _equipment[equippableItem.Slot].Data.ItemId, Data = returnItem}, _parent);
                    }

                    _equipment[equippableItem.Slot] = null;
                }
                var equipped = new EquippedItem(_parent, equippableItem, msg.Item.ItemId);
                _equipment[equippableItem.Slot] = equipped;
                this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
            }
            else
            {
                this.SendMessageTo(new AddItemByIdMessage{Data = item, ItemId = msg.Item.ItemId}, _parent);
            }
        }

        private void QueryEquipment(QueryEquipmentMessage msg)
        {
            msg.DoAfter.Invoke(_equipment.Values.Where(e => e != null).Select(e => e.Data).ToArray());
        }

        private void ClientUnEquipItemFromSlot(ClientUnEquipItemFromSlotMessage msg)
        {
            if (_equipment[msg.Slot] != null)
            {
                var availableSlots = 0;
                this.SendMessageTo(new QueryMaxInventorySlotsMessage { DoAfter = playerSlots => availableSlots = playerSlots }, _parent);
                if (availableSlots > 0)
                {
                    var item = ItemService.GetItemByName(_equipment[msg.Slot].Data.Item);
                    if (item != null)
                    {
                        _equipment[msg.Slot].Unequip(_parent);
                        this.SendMessageTo(new AddItemByIdMessage { ItemId = _equipment[msg.Slot].Data.ItemId, Data = item}, _parent);
                        _equipment[msg.Slot] = null;
                    }
                }
                else
                {
                    this.SendMessageTo(new RegisterPlayerEventMessage
                    {
                        Event = new PlayerEvent
                        {
                            EventType = PlayerEventType.Warning,
                            EventMessage = "Inventory is full"
                        }
                    }, _parent);
                }
            }

        }

        private void SetCharacterEquipment(SetCharacterEquipmentMessage msg)
        {
            var equipment = msg.Equipment;
            for (var i = 0; i < equipment.Length; i++)
            {
                if (_equipment[equipment[i].Slot] == null)
                {
                    var item = ItemService.GetItemByName(equipment[i].Item);
                    if (item != null && item is EquippableItemData equippable)
                    {
                        var equippedItem = new EquippedItem(_parent, equippable, equipment[i].ItemId);
                        _equipment[equipment[i].Slot] = equippedItem;
                    }
                }
            }
        }
    }
}