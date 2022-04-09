using System.Linq;
using AncibleCoreCommon;
using AncibleCoreCommon.CommonData.Client;
using AncibleCoreCommon.CommonData.Items;
using AncibleCoreServer.Services.Items;
using AncibleCoreServer.Services.Maps;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class PlayerInteractionTrait : ObjectTrait
    {
        private const string NAME = "Player Interaction Trait";

        private WorldObject _currentObject = null;

        private string _playerId = string.Empty;

        private ObjectState _objectState = ObjectState.Active;

        public PlayerInteractionTrait(string playerId)
        {
            Name = NAME;
            _playerId = playerId;
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            this.SubscribeWithFilter<ClientInteractWithObjectRequestMessage>(ClientInteractWithObject, _playerId);
            this.SubscribeWithFilter<ClientBuyItemFromShopRequestMessage>(ClientBuyItemFromShop, _playerId);
            this.SubscribeWithFilter<ClientSellItemToShopRequestMessage>(ClientSellItemToShop, _playerId);
            this.SubscribeWithFilter<ClientFinishInteractionRequestMessage>(ClientInteractionFinishedRequest, _playerId);
            this.SubscribeWithFilter<ClientLootItemRequestMessage>(ClientLootItem, _playerId);
            this.SubscribeWithFilter<ClientLootAllRequestMessage>(ClientLootAll, _playerId);
            
            _parent.SubscribeWithFilter<UpdateObjectStateMessage>(UpdateObjectState, _instanceId);
            _parent.SubscribeWithFilter<InteractionFinishedMessage>(InteractionFinished, _instanceId);
        }

        private void ClientInteractWithObject(ClientInteractWithObjectRequestMessage msg)
        {
            if (_objectState != ObjectState.Casting && _objectState != ObjectState.Moving && _objectState != ObjectState.Dead)
            {
                var obj = ObjectManagerService.GetObjectFromId(msg.ObjectId);
                if (obj != null)
                {
                    if (obj.Interactions.Contains(msg.Interaction))
                    {
                        var objTiles = MapService.GetMapTilesInArea(obj.Map, obj.Tile, ObjectManagerService.DefaultInteractionRange).Where(t => !t.Obstacle).ToArray();
                        if (objTiles.FirstOrDefault(t => t == _parent.Tile) != null)
                        {
                            var errorMessage = string.Empty;
                            this.SendMessageTo(new InteractWithObjectMessage { Owner = _parent, Type = msg.Interaction, OnError = message => errorMessage = message}, obj);
                            if (string.IsNullOrEmpty(errorMessage))
                            {
                                this.SendMessageTo(new SetObjectStateMessage { State = ObjectState.Interaction }, _parent);
                                _currentObject = obj;
                                this.SendMessageTo(new ClientInteractWithObjectResultMessage { Success = true }, _parent);
                            }
                            else
                            {
                                this.SendMessageTo(new ClientInteractWithObjectResultMessage { Success = false, Message = errorMessage}, _parent);
                            }
                            
                        }
                        else
                        {
                            this.SendMessageTo(new ClientInteractWithObjectResultMessage { Success = false, Message = "Too far away" }, _parent);
                        }
                    }
                    else
                    {
                        this.SendMessageTo(new ClientInteractWithObjectResultMessage { Success = false, Message = "Unavailable" }, _parent);
                    }

                }
                else
                {
                    this.SendMessageTo(new ClientInteractWithObjectResultMessage { Success = false, Message = "Unavailable" }, _parent);
                }

            }
            else
            {
                this.SendMessageTo(new ClientInteractWithObjectResultMessage { Success = false, Message = "You are currently busy" }, _parent);
            }
        }

        private void ClientBuyItemFromShop(ClientBuyItemFromShopRequestMessage msg)
        {
            if (_currentObject != null && _currentObject.Id == msg.ObjectId)
            {
                var emptySpace = false;
                this.SendMessageTo(new QueryAvailableInventorySlotsMessage{DoAfter = slots => emptySpace = slots >= msg.Stack}, _parent);
                if (emptySpace)
                {
                    ShopItemData item = null;
                    this.SendMessageTo(new QueryShopItemByIdMessage
                    {
                        ShopId = msg.ShopId,
                        DoAfter = shopItem =>
                        {
                            item = shopItem;
                        }
                    }, _currentObject);
                    if (item != null)
                    {
                        var itemData = ItemService.GetItemByName(item.Item);
                        if (itemData != null)
                        {
                            var playerGold = 0;
                            this.SendMessageTo(new QueryGoldMessage { DoAfter = gold => playerGold = gold }, _parent);
                            var cost = item.Cost * msg.Stack;
                            if (cost <= playerGold)
                            {
                                this.SendMessageTo(new RemoveGoldMessage { Amount = cost }, _parent);
                                this.SendMessageTo(new AddItemToInventoryMessage{Item = itemData, Stack = item.Stack * msg.Stack}, _parent);
                                this.SendMessageTo(new ClientBuyItemFromShopResultMessage { Success = true, Item = itemData.Name, Cost = cost, Stack = item.Stack * msg.Stack}, _parent);
                            }
                            else
                            {
                                this.SendMessageTo(new ClientBuyItemFromShopResultMessage{Success = false, Message = "Not enough gold"}, _parent);
                            }
                        }
                        else
                        {
                            this.SendMessageTo(new ClientBuyItemFromShopResultMessage { Success = false, Message = "Invalid Item" }, _parent);
                        }
                    }
                    else
                    {
                        this.SendMessageTo(new ClientBuyItemFromShopResultMessage { Success = false, Message = "Invalid Item" }, _parent);
                    }
                }
                else
                {
                    this.SendMessageTo(new ClientBuyItemFromShopResultMessage { Success = false, Message = "Not enough space" }, _parent);
                }

            }
            else
            {
                this.SendMessageTo(new ClientBuyItemFromShopResultMessage { Success = false, Message = "You are not shopping" }, _parent);
            }
        }

        private void ClientSellItemToShop(ClientSellItemToShopRequestMessage msg)
        {
            if (_currentObject != null && _currentObject.Id == msg.ObjectId)
            {
                var isShop = false;
                this.SendMessageTo(new QueryIsShopMessage{DoAfter = () => { isShop = true; }}, _currentObject);
                if (isShop)
                {
                    ClientItemData clientItem = null;
                    this.SendMessageTo(new QueryInventoryItemByIdMessage{ItemId = msg.ItemId, DoAfter = playerItem => clientItem = playerItem}, _parent);
                    if (clientItem != null)
                    {
                        var itemData = ItemService.GetItemByName(clientItem.Item);
                        if (itemData != null)
                        {
                            if (itemData.SellValue >= 0)
                            {
                                var sellStack = msg.Stack;
                                if (sellStack > clientItem.Stack)
                                {
                                    sellStack = clientItem.Stack;
                                }

                                var gold = sellStack * itemData.SellValue;
                                this.SendMessageTo(new AddGoldMessage{Amount = gold}, _parent);
                                this.SendMessageTo(new RemoveInventoryItemByIdMessage{ItemId = clientItem.ItemId, Stack = sellStack}, _parent);
                                this.SendMessageTo(new ClientSellItemToShopResultMessage { Success = true, Item = itemData.Name, Amount = gold, Stack = sellStack}, _parent);
                            }
                            else
                            {
                                this.SendMessageTo(new ClientSellItemToShopResultMessage { Success = false, Message = "Cannot sell that item", Item = itemData.Name}, _parent);
                            }
                        }
                        else
                        {
                            this.SendMessageTo(new ClientSellItemToShopResultMessage { Success = false, Message = "Invalid Item" }, _parent);
                        }
                    }
                    else
                    {
                        this.SendMessageTo(new ClientSellItemToShopResultMessage { Success = false, Message = "Invalid Item" }, _parent);
                    }
                }
                else
                {
                    this.SendMessageTo(new ClientSellItemToShopResultMessage{Success = false, Message = "Invalid object"}, _parent);
                }
            }
        }

        private void ClientInteractionFinishedRequest(ClientFinishInteractionRequestMessage msg)
        {
            if (_currentObject != null)
            {
                this.SendMessageTo(new FinishInteractionMessage{Owner = _parent}, _currentObject);
                _currentObject = null;
                this.SendMessageTo(new SetObjectStateMessage{State = ObjectState.Active}, _parent);
                this.SendMessageTo(new ClientFinishInteractionResultMessage{Success = true}, _parent);
            }
            else
            {
                this.SendMessageTo(new ClientFinishInteractionResultMessage{Success = false, Message = "You're currently not looting"}, _parent);
            }
        }

        private void ClientLootItem(ClientLootItemRequestMessage msg)
        {
            if (_currentObject != null && _currentObject.Id == msg.ObjectId)
            {
                var errorMessage = string.Empty;
                this.SendMessageTo(new LootItemMessage{ItemId = msg.ItemId, Object = _parent, OnError = message => errorMessage = message}, _currentObject);
                this.SendMessageTo(string.IsNullOrEmpty(errorMessage)
                        ? new ClientLootItemResultMessage {Success = true}
                        : new ClientLootItemResultMessage {Success = false, Message = errorMessage}, _parent);
            }
            else
            {
                this.SendMessageTo(new ClientFinishInteractionResultMessage { Success = false, Message = "You're currently not looting" }, _parent);
            }
        }

        private void ClientLootAll(ClientLootAllRequestMessage msg)
        {
            if (_currentObject != null && _currentObject.Id == msg.ObjectId)
            {
                var errorMessage = string.Empty;
                this.SendMessageTo(new LootAllMessage { Object = _parent, OnError = message => errorMessage = message }, _currentObject);
                this.SendMessageTo(string.IsNullOrEmpty(errorMessage)
                    ? new ClientLootItemResultMessage { Success = true }
                    : new ClientLootItemResultMessage { Success = false, Message = errorMessage }, _parent);
            }
        }

        private void UpdateObjectState(UpdateObjectStateMessage msg)
        {
            _objectState = msg.State;
            if (_objectState != ObjectState.Interaction && _currentObject != null)
            {
                this.SendMessageTo(new FinishInteractionMessage{Owner = _parent}, _currentObject);
                _currentObject = null;
                this.SendMessageTo(new ClientFinishInteractionResultMessage { Success = true }, _parent);
            }
        }

        private void InteractionFinished(InteractionFinishedMessage msg)
        {
            if (_currentObject != null && _currentObject == msg.Object)
            {
                this.SendMessageTo(new SetObjectStateMessage{State = ObjectState.Active}, _parent);
                this.SendMessageTo(new ClientFinishInteractionResultMessage { Success = true }, _parent);
            }
            
        }
    }
}