using System.Collections.Generic;
using System.Linq;
using AncibleCoreCommon;
using AncibleCoreCommon.CommonData;
using AncibleCoreServer.Services.Items;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class LootableTrait : ObjectTrait
    {
        public const string NAME = "Lootable Trait";

        private List<ItemStack> _items = new List<ItemStack>();
        private int _gold = 0;

        private WorldObject[] _validLooters = new WorldObject[0];
        private WorldObject _currentLooter = null;

        private TickTimer _aliveTimer = null;

        public LootableTrait(ItemStack[] items, int gold, List<WorldObject> validLooters)
        {
            Name = NAME;
            _validLooters = validLooters.ToArray();
            
            _items = items.ToList();
            _gold = gold;
            _aliveTimer = new TickTimer(LootTableService.ChestTicks, () =>
            {
                _aliveTimer = null;
                if (_currentLooter != null)
                {
                    this.SendMessageTo(new InteractionFinishedMessage { Object = _parent }, _currentLooter);
                    _currentLooter = null;
                }
                _validLooters = new WorldObject[0];
                ObjectManagerService.RemoveObjectFromWorld(_parent.Map, _parent);
            });
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            if (!_parent.Interactions.Contains(InteractionType.Loot))
            {
                _parent.Interactions.Add(InteractionType.Loot);
            }
            _parent.VisibleList.AddRange(_validLooters.Select(v => v.Id));
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _parent.SubscribeWithFilter<InteractWithObjectMessage>(InteractWithObject, _instanceId);
            _parent.SubscribeWithFilter<LootItemMessage>(LootItem, _instanceId);
            _parent.SubscribeWithFilter<LootAllMessage>(LootAll, _instanceId);
            _parent.SubscribeWithFilter<FinishInteractionMessage>(FinishInteraction, _instanceId);
        }

        private void InteractWithObject(InteractWithObjectMessage msg)
        {
            if (msg.Type == InteractionType.Loot)
            {
                if (_currentLooter != null && _currentLooter != msg.Owner)
                {
                    msg.OnError?.Invoke($"Someone else is currently looting {_parent.DisplayName}");
                }
                else if (!_validLooters.Contains(msg.Owner))
                {
                    msg.OnError?.Invoke($"You do not have permission to loot {_parent.DisplayName}");
                }
                else if (_currentLooter != null && _currentLooter == msg.Owner)
                {
                    msg.Owner.SendMessageTo(new LootAllMessage{Object = msg.Owner, OnError = msg.OnError}, _parent);
                }
                else
                {
                    if (_gold > 0)
                    {
                        var goldPerLooter = _gold / _validLooters.Length;
                        if (goldPerLooter <= 0)
                        {
                            goldPerLooter = 1;
                        }

                        var addGoldMessage = new AddGoldMessage { Amount = goldPerLooter };
                        for (var i = 0; i < _validLooters.Length; i++)
                        {
                            this.SendMessageTo(addGoldMessage, _validLooters[i]);
                        }

                        _gold = 0;
                    }

                    if (_items.Count > 0)
                    {
                        _currentLooter = msg.Owner;
                        this.SendMessageTo(new ClientShowLootWindowMessage { ObjectId = _parent.Id, Loot = _items.Select(i => i.ToClientLootData()).ToArray() }, _currentLooter);
                        _aliveTimer.TogglePause();
                    }
                    else
                    {
                        this.SendMessageTo(new InteractionFinishedMessage { Object = _parent }, msg.Owner);
                        _validLooters = new WorldObject[0];
                        _aliveTimer.Destroy();
                        _aliveTimer = null;
                        ObjectManagerService.RemoveObjectFromWorld(_parent.Map, _parent);
                    }

                }
            }
            else
            {
                msg.OnError?.Invoke("Invalid");
            }
            
        }

        private void LootItem(LootItemMessage msg)
        {
            if (_currentLooter != null && msg.Object == _currentLooter)
            {
                var item = _items.FirstOrDefault(i => i.Id == msg.ItemId);
                if (item != null)
                {
                    var returnStack = 0;
                    this.SendMessageTo(new AddItemToInventoryMessage
                    {
                        Item = item.Item, Stack = item.Stack, ReturnStack = stack => returnStack = stack, Announce = true
                    }, _currentLooter);
                    if (returnStack > 0)
                    {
                        item.Stack = returnStack;
                    }
                    else
                    {
                        _items.Remove(item);
                        if (_items.Count <= 0)
                        {
                            this.SendMessageTo(new InteractionFinishedMessage{Object = _parent}, _currentLooter);
                            _currentLooter = null;
                            _validLooters = new WorldObject[0];
                            _aliveTimer.Destroy();
                            _aliveTimer = null;
                            ObjectManagerService.RemoveObjectFromWorld(_parent.Map, _parent);
                        }
                        else
                        {
                            this.SendMessageTo(new ClientShowLootWindowMessage { ObjectId = _parent.Id, Loot = _items.Select(i => i.ToClientLootData()).ToArray() }, _currentLooter);
                        }
                    }
                }
            }
        }

        private void LootAll(LootAllMessage msg)
        {
            if (_currentLooter != null && msg.Object == _currentLooter)
            {
                var items = _items.ToArray();
                var addItemToInventoryMsg = new AddItemToInventoryMessage {Announce = true};
                for (var i = 0; i < items.Length; i++)
                {
                    var returnStack = 0;
                    addItemToInventoryMsg.Item = items[i].Item;
                    addItemToInventoryMsg.Stack = items[i].Stack;
                    addItemToInventoryMsg.ReturnStack = stack => returnStack = stack;
                    this.SendMessageTo(addItemToInventoryMsg, _currentLooter);
                    if (returnStack > 0)
                    {
                        items[i].Stack = returnStack;
                        break;
                    }
                    else
                    {
                        _items.Remove(items[i]);
                    }
                }

                if (_items.Count <= 0)
                {
                    this.SendMessageTo(new InteractionFinishedMessage { Object = _parent }, _currentLooter);
                    _currentLooter = null;
                    _validLooters = new WorldObject[0];
                    _aliveTimer.Destroy();
                    _aliveTimer = null;
                    ObjectManagerService.RemoveObjectFromWorld(_parent.Map, _parent);
                }
                else
                {
                    this.SendMessageTo(new ClientShowLootWindowMessage { ObjectId = _parent.Id, Loot = _items.Select(i => i.ToClientLootData()).ToArray() }, _currentLooter);
                }
            }
        }

        private void FinishInteraction(FinishInteractionMessage msg)
        {
            if (_currentLooter != null && _currentLooter == msg.Owner)
            {
                _currentLooter = null;
                _aliveTimer.TogglePause();
            }
        }

        public override void Destroy()
        {
            if (_aliveTimer != null)
            {
                _aliveTimer.Destroy();
                _aliveTimer = null;
            }
            base.Destroy();
        }
    }
}