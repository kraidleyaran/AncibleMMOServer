using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using AncibleCoreCommon;
using AncibleCoreCommon.CommonData.Client;
using AncibleCoreCommon.CommonData.Combat;
using AncibleCoreCommon.CommonData.Items;
using AncibleCoreCommon.CommonData.PlayerEvent;
using AncibleCoreServer.Data;
using AncibleCoreServer.Services.Items;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class PlayerInventoryTrait : ObjectTrait
    {
        public const string TRAIT_NAME = "Player Inventory Trait";
        private Dictionary<int, ClientItemData> _items = new Dictionary<int, ClientItemData>();
        private int _maxSlots = 0;

        private string _playerId = string.Empty;
        private int _gold = 0;

        public PlayerInventoryTrait(string playerId, WorldItem[] items, int maxSlots, int gold)
        {
            Name = TRAIT_NAME;
            _playerId = playerId;
            _maxSlots = maxSlots;
            _gold = gold;
            for (var i = 0; i < maxSlots; i++)
            {
                _items.Add(i, null);
            }

            for (var i = 0; i < items.Length; i++)
            {
                if (_items.ContainsKey(items[i].Slot))
                {
                    _items[items[i].Slot] = items[i].ToData();
                }
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            SubscribeToMessages();
        }

        private int AddItem(ItemData item, int stack)
        {
            var existingItems = _items.Where(kv => kv.Value != null && kv.Value.Item == item.Name && kv.Value.Stack < item.MaxStack).OrderByDescending(kv => kv.Value.Stack).ToArray();
            var remainingStack = stack;
            if (existingItems.Length > 0)
            {
                for (var i = 0; i < existingItems.Length; i++)
                {
                    if (remainingStack + existingItems[i].Value.Stack > item.MaxStack)
                    {
                        _items[existingItems[i].Key].Stack = item.MaxStack;
                        remainingStack -= item.MaxStack - existingItems[i].Value.Stack;
                    }
                    else
                    {
                        _items[existingItems[i].Key].Stack = remainingStack + existingItems[i].Value.Stack;
                        remainingStack = 0;
                    }
                    
                    if (remainingStack <= 0)
                    {
                        break;
                    }
                }

                if (remainingStack > 0)
                {
                    remainingStack = AddItem(item, remainingStack);
                }
            }
            else
            {
                var availableSlots = _items.Where(kv => kv.Value == null).ToArray().OrderBy(kv => kv.Key).ToArray();
                if (availableSlots.Length > 0)
                {
                    for (var i = 0; i < availableSlots.Length; i++)
                    {
                        var additionalStack = remainingStack;
                        if (additionalStack > item.MaxStack)
                        {
                            additionalStack = item.MaxStack;
                            remainingStack -= additionalStack;
                        }
                        else
                        {
                            remainingStack = 0;
                        }

                        _items[availableSlots[i].Key] = new ClientItemData { Item = item.Name, Slot = availableSlots[i].Key, Stack = additionalStack};
                        if (remainingStack <= 0)
                        {
                            break;
                        }
                    }

                    if (remainingStack > 0)
                    {
                        
                        //TODO: Send inventory is full;
                    }
                }
            }

            return remainingStack;
        }

        private void RemoveItem(string item, int stack)
        {
            var existingItems = _items.Where(kv => kv.Value != null && kv.Value.Item == item).OrderBy(kv => kv.Value.Stack).ToArray();
            var remainingStack = stack;
            for (var i = 0; i < existingItems.Length; i++)
            {
                var newStack = existingItems[i].Value.Stack - remainingStack;
                if (newStack <= 0)
                {
                    remainingStack -= existingItems[i].Value.Stack;
                    _items[existingItems[i].Key] = null;
                }
                else
                {
                    _items[existingItems[i].Key].Stack = newStack;
                    remainingStack = 0;
                }

                if (remainingStack <= 0)
                {
                    break;
                }
            }
        }

        private void RemoveItemById(string id, int stack)
        {
            var existingItem = _items.FirstOrDefault(kv => kv.Value != null && kv.Value.ItemId == id);
            var remainingStack = stack;
            var itemName = existingItem.Value.Item;
            if (existingItem.Value != null)
            {
                var newStack = existingItem.Value.Stack - remainingStack;
                if (newStack <= 0)
                {
                    remainingStack -= existingItem.Value.Stack;
                    existingItem.Value.Stack = 0;
                    _items[existingItem.Key] = null;
                }
                else
                {
                    _items[existingItem.Key].Stack = newStack;
                    remainingStack = 0;
                }
            }

            if (remainingStack > 0)
            {
                RemoveItem(itemName, remainingStack);
            }
        }

        private void SubscribeToMessages()
        {
            this.SubscribeWithFilter<ClientUseItemMessage>(ClientUseItem, _playerId);
            this.SubscribeWithFilter<ClientEquipItemMessage>(ClientEquipItem, _playerId);
            this.SubscribeWithFilter<ClientMoveItemToSlotRequestMessage>(ClientMoveItemToSlotRequest, _playerId);

            _parent.SubscribeWithFilter<AddItemToInventoryMessage>(AddItemToInventory, _instanceId);
            _parent.SubscribeWithFilter<RemoveItemFromInventoryByNameMessage>(RemoveItemFromInventoryByName, _instanceId);
            _parent.SubscribeWithFilter<QueryInventoryMessage>(QueryInventory, _instanceId);
            _parent.SubscribeWithFilter<QueryInventoryItemsByNameMessage>(QueryInventoryItemsByName, _instanceId);
            _parent.SubscribeWithFilter<AddGoldMessage>(AddGold, _instanceId);
            _parent.SubscribeWithFilter<RemoveGoldMessage>(RemoveGold, _instanceId);
            _parent.SubscribeWithFilter<QueryGoldMessage>(QueryGold, _instanceId);
            _parent.SubscribeWithFilter<QueryMaxInventorySlotsMessage>(QueryMaxInventorySlots, _instanceId);
            _parent.SubscribeWithFilter<AddItemByIdMessage>(AddItemById, _instanceId);
            _parent.SubscribeWithFilter<QueryAvailableInventorySlotsMessage>(QueryAvailableInventorySlots, _instanceId);
            _parent.SubscribeWithFilter<QueryInventoryItemByIdMessage>(QueryInventoryItemById, _instanceId);
            _parent.SubscribeWithFilter<RemoveInventoryItemByIdMessage>(RemoveInventoryItemById, _instanceId);
        }

        private void AddItemToInventory(AddItemToInventoryMessage msg)
        {
            var returnStack = AddItem(msg.Item, msg.Stack);
            if (returnStack > 0)
            {
                msg.ReturnStack?.Invoke(returnStack);
            }

            if (msg.Announce)
            {
                var stack = msg.Stack - returnStack;
                if (stack > 0)
                {
                    this.SendMessageTo(new RegisterPlayerEventMessage{Event = new PickupItemPlayerEvent{Stack = stack, ItemId = msg.Item.Name}}, _parent);
                }
                
            }
            this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
        }

        private void AddItemById(AddItemByIdMessage msg)
        {
            var availableSlots = _items.Where(kv => kv.Value == null).ToArray().OrderBy(kv => kv.Key).ToArray();
            if (availableSlots.Length > 0)
            {
                _items[availableSlots[0].Key] = new ClientItemData
                {
                    Item = msg.Data.Name,
                    ItemId = msg.ItemId,
                    Slot = availableSlots[0].Key,
                    Stack = 1
                };
            }
        }

        private void RemoveItemFromInventoryByName(RemoveItemFromInventoryByNameMessage msg)
        {
            RemoveItem(msg.Item, msg.Stack);
            this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
        }

        private void QueryInventory(QueryInventoryMessage msg)
        {
            msg.DoAfter.Invoke(_items.Values.Where(i => i != null).ToArray());
        }

        private void QueryInventoryItemsByName(QueryInventoryItemsByNameMessage msg)
        {
            var predicateItems = msg.Items.ToList();
            var items = _items.Values.Where(i => i != null && predicateItems.Contains(i.Item)).ToArray();
            msg.DoAfter.Invoke(items);
        }

        private void QueryMaxInventorySlots(QueryMaxInventorySlotsMessage msg)
        {
            msg.DoAfter.Invoke(_maxSlots);
        }

        private void AddGold(AddGoldMessage msg)
        {
            _gold += msg.Amount;
            this.SendMessageTo(new RegisterPlayerEventMessage{Event = new PlayerGoldEvent{Amount = msg.Amount}}, _parent);
            this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
        }

        private void RemoveGold(RemoveGoldMessage msg)
        {
            _gold -= msg.Amount;
            if (_gold < 0)
            {
                _gold = 0;
            }
            this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
        }

        private void QueryGold(QueryGoldMessage msg)
        {
            msg.DoAfter.Invoke(_gold);
        }

        private void ClientUseItem(ClientUseItemMessage msg)
        {
            var statusEffects = new List<StatusEffectType>();
            this.SendMessageTo(new QueryStatusEffectsMessage{DoAfter = effect => statusEffects.Add(effect)}, _parent);
            if (statusEffects.Contains(StatusEffectType.Daze) || statusEffects.Contains(StatusEffectType.Sleep))
            {
                var effects = statusEffects.Where(e => e == StatusEffectType.Daze || e == StatusEffectType.Pacify).ToArray();
                var effectsString = string.Empty;
                for (var i = 0; i < effects.Length; i++)
                {
                    effectsString = i < effects.Length - 1 ? $"{effectsString}{effects[i].ToPastTenseEffectString()}," : $"{effectsString}{effects[i].ToPastTenseEffectString()}";
                }
                this.SendMessageTo(new RegisterPlayerEventMessage{Event = new PlayerEvent{EventType = PlayerEventType.Warning, EventMessage = $"Cannot use an item while {effectsString}"}}, _parent );
                return;
            }
            else
            {
                var globalCooldown = 0;
                this.SendMessageTo(new QueryGlobalCooldownMessage { DoAfter = cooldown => globalCooldown = cooldown }, _parent);
                if (globalCooldown > 0)
                {
                    this.SendMessageTo(new RegisterPlayerEventMessage { Event = new PlayerEvent { EventMessage = "On Cooldown", EventType = PlayerEventType.Warning } }, _parent);
                    return;
                }

                ClientItemData clientItem = null;
                if (!string.IsNullOrEmpty(msg.ItemId))
                {
                    clientItem = _items.Values.FirstOrDefault(i => i != null && i.ItemId == msg.ItemId);
                }
                if (clientItem == null && !string.IsNullOrEmpty(msg.Name))
                {
                    clientItem = _items.Values.FirstOrDefault(i => i != null && i.Item == msg.Name);
                }
                if (clientItem != null)
                {
                    var itemData = ItemService.GetItemByName(clientItem.Item);
                    if (itemData != null && itemData.Type == ItemType.Useable)
                    {
                        if (itemData is UseableItemData useableItem)
                        {
                            if (clientItem.Stack >= useableItem.UseOnStack)
                            {
                                var traits = useableItem.ApplyToUser.Select(TraitService.GetTrait).Where(t => t != null).ToArray();
                                var addTraitToObjMsg = new AddTraitToObjectMessage();
                                for (var i = 0; i < traits.Length; i++)
                                {
                                    addTraitToObjMsg.Trait = traits[i];
                                    _parent.SendMessageTo(addTraitToObjMsg, _parent);
                                }

                                RemoveItemById(clientItem.ItemId, useableItem.UseOnStack);
                                this.SendMessageTo(TriggerGlobalCooldownMessage.INSTANCE, _parent);
                                this.SendMessageTo(new RegisterPlayerEventMessage { Event = new PlayerUsedItemEvent { EventMessage = "You used", Item = itemData.Name } }, _parent);
                                this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
                            }
                            else
                            {
                                this.SendMessageTo(new RegisterPlayerEventMessage { Event = new PlayerEvent { EventMessage = "Not Enough Stack for Use", EventType = PlayerEventType.Warning } }, _parent);
                            }

                        }
                        else
                        {
                            this.SendMessageTo(new RegisterPlayerEventMessage { Event = new PlayerEvent { EventMessage = "Invalid Item", EventType = PlayerEventType.Warning } }, _parent);
                        }

                    }
                    else
                    {
                        this.SendMessageTo(new RegisterPlayerEventMessage { Event = new PlayerEvent { EventMessage = "Invalid Item", EventType = PlayerEventType.Warning } }, _parent);
                    }
                }
                else
                {
                    this.SendMessageTo(new RegisterPlayerEventMessage { Event = new PlayerEvent { EventMessage = "Invalid Item", EventType = PlayerEventType.Warning } }, _parent);
                }
            }

        }

        private void ClientEquipItem(ClientEquipItemMessage msg)
        {
            var globalCooldown = 0;
            this.SendMessageTo(new QueryGlobalCooldownMessage { DoAfter = cooldown => globalCooldown = cooldown }, _parent);
            if (globalCooldown > 0)
            {
                this.SendMessageTo(new RegisterPlayerEventMessage { Event = new PlayerEvent { EventMessage = "On Cooldown", EventType = PlayerEventType.Warning } }, _parent);
                return;
            }
            var item = _items.Values.FirstOrDefault(i => i != null && i.ItemId == msg.ItemId);
            if (item != null)
            {
                RemoveItemById(item.ItemId, 1);
                this.SendMessageTo(new EquipItemMessage{Item = item}, _parent);
                this.SendMessageTo(TriggerGlobalCooldownMessage.INSTANCE, _parent);
            }
        }

        private void QueryAvailableInventorySlots(QueryAvailableInventorySlotsMessage msg)
        {
            msg.DoAfter.Invoke(_items.Values.Count(v => v == null));
        }

        private void QueryInventoryItemById(QueryInventoryItemByIdMessage msg)
        {
            var item = _items.Values.FirstOrDefault(i => i != null && i.ItemId == msg.ItemId);
            if (item != null)
            {
                msg.DoAfter.Invoke(item);
            }
        }

        private void RemoveInventoryItemById(RemoveInventoryItemByIdMessage msg)
        {
            RemoveItemById(msg.ItemId, msg.Stack);
        }

        private void ClientMoveItemToSlotRequest(ClientMoveItemToSlotRequestMessage msg)
        {
            var globalCooldown = 0;
            this.SendMessageTo(new QueryGlobalCooldownMessage { DoAfter = cooldown => globalCooldown = cooldown }, _parent);
            if (globalCooldown > 0)
            {
                this.SendMessageTo(new RegisterPlayerEventMessage { Event = new PlayerEvent { EventMessage = "On Cooldown", EventType = PlayerEventType.Warning } }, _parent);
                return;
            }
            var item = _items.Values.FirstOrDefault(i => i != null && i.ItemId == msg.ItemId);
            if (item != null)
            {
                if (_items.TryGetValue(msg.Slot, out var itemAtSlot))
                {
                    if (itemAtSlot != null)
                    {
                        if (item.Item == itemAtSlot.Item)
                        {
                            var itemData = ItemService.GetItemByName(item.Item);
                            if (item.Stack < itemData.MaxStack)
                            {
                                var newStack = itemAtSlot.Stack + item.Stack;
                                if (newStack > itemData.MaxStack)
                                {
                                    item.Stack = newStack - itemData.MaxStack;
                                    newStack = itemData.MaxStack;
                                    itemAtSlot.Stack = newStack;
                                }
                                else
                                {
                                    itemAtSlot.Stack = newStack;
                                    RemoveItemById(item.ItemId, item.Stack);
                                }
                            }
                            else
                            {
                                itemAtSlot.Slot = item.Slot;
                                item.Slot = msg.Slot;
                                _items[item.Slot] = item;
                                _items[itemAtSlot.Slot] = itemAtSlot;
                            }
                        }
                        else
                        {
                            itemAtSlot.Slot = item.Slot;
                            item.Slot = msg.Slot;
                            _items[item.Slot] = item;
                            _items[itemAtSlot.Slot] = itemAtSlot;
                        }
                    }
                    else
                    {
                        _items[item.Slot] = null;
                        item.Slot = msg.Slot;
                        _items[item.Slot] = item;
                        
                    }
                    this.SendMessageTo(TriggerGlobalCooldownMessage.INSTANCE, _parent);
                    this.SendMessageTo(new ClientMoveItemToSlotResultMessage { Success = true }, _parent);
                    this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
                    
                }
                else
                {
                    this.SendMessageTo(new ClientMoveItemToSlotResultMessage { Success = false, Message = "Invalid Bag Slot" }, _parent);
                }

            }
            else
            {
                this.SendMessageTo(new ClientMoveItemToSlotResultMessage{Success = false, Message = "Invalid Item"}, _parent);
            }
        }
    }
}