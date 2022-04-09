using System;
using System.Linq;
using AncibleCoreCommon;
using AncibleCoreServer.Data;
using AncibleCoreServer.Services.Ability;
using AncibleCoreServer.Services.CharacterClass;
using AncibleCoreServer.Services.Combat;
using AncibleCoreServer.Services.Database;
using AncibleCoreServer.Services.Items;
using AncibleCoreServer.Services.Maps;
using AncibleCoreServer.Services.ObjectManager;
using LiteDB;
using LiteDB.Engine;
using MessageBusLib;

namespace AncibleCoreServer.Services.ClientManager
{
    public class PlayerClient
    {
        public PlayerClient(int networkId, AuthenticationSession session)
        {
            NetworkId = networkId;
            Session = session;
            WorldId = Guid.NewGuid().ToString();
        }

        public string User;
        public int NetworkId;
        public string WorldId;
        public int UserId;
        public AuthenticationSession Session;
        public WorldObject Character;
        public LiteDatabase UserDatabase;
        

        private int _ticksSinceLastCheckIn = 0;
        private bool _disconnecting = false;

        public void Authenticate()
        {
            Session.SetAuthenticationState(true);
            SubscribeToMessages();
        }

        public string GetUser()
        {
            return $"{User} => {(Character != null ? $"{Character.DisplayName}-{Character.Map}" : "Logged In")}";
        }

        private void SubscribeToMessages()
        {
            this.Subscribe<WorldTickMessage>(WorldTick);
            this.Subscribe<SaveDataMessage>(SaveData);

            this.SubscribeWithFilter<ClientCreateCharacterRequestMessage>(ClientCreateCharacterRequest, WorldId);
            this.SubscribeWithFilter<ClientCharacterRequestMessage>(ClientCharacterRequest, WorldId);
            this.SubscribeWithFilter<ClientCheckInResponseMessage>(ClientCheckInResponse,WorldId);
            this.SubscribeWithFilter<ClientEnterWorldWithCharacterRequestMessage>(ClientEnterWorldWithCharacterRequest, WorldId);
            this.SubscribeWithFilter<DisconnectClientMessage>(DisconnectClient, WorldId);
        }

        private void SubscribePlayerObjectMessages()
        {
            this.Subscribe<UpdateClientsTickMessage>(UpdateClientsTick);
            Character.SubscribeWithFilter<ClientMovementResponseMessage>(ClientMovementResponse, WorldId);
            Character.SubscribeWithFilter<ClientCharacterUpdateMessage>(ClientCharacterUpdate, WorldId);
            Character.SubscribeWithFilter<ClientUseAbilityResultMessage>(ClientUseAbilityResult, WorldId);
            Character.SubscribeWithFilter<CastCancelledMessage>(CastCancelled, WorldId);
            Character.SubscribeWithFilter<ClientPlayerDeadMessage>(ClientPlayerDead, WorldId);
            Character.SubscribeWithFilter<ClientPlayerRespawnMessage>(ClientPlayerRespawn, WorldId);
            Character.SubscribeWithFilter<ClientCastFailedMessage>(ClientCastFailed, WorldId);
            Character.SubscribeWithFilter<ClientPlayerEventUpdateMessage>(ClientPlayerEventUpdate, WorldId);
            Character.SubscribeWithFilter<ClientBuyItemFromShopResultMessage>(ClientBuyItemFromShopResult, WorldId);
            Character.SubscribeWithFilter<ClientSellItemToShopResultMessage>(ClientSellItemFromShopResult, WorldId);
            Character.SubscribeWithFilter<ClientInteractWithObjectResultMessage>(ClientInteractWithObjectResult, WorldId);
            Character.SubscribeWithFilter<ClientFinishInteractionResultMessage>(ClientFinishInteractionResult, WorldId);
            Character.SubscribeWithFilter<ClientShowShopMessage>(ClientShowShop, WorldId);
            Character.SubscribeWithFilter<ClientShowLootWindowMessage>(ClientShowLootWindow, WorldId);
            Character.SubscribeWithFilter<ClientMoveItemToSlotResultMessage>(ClientMoveItemToSlotResult, WorldId);
            Character.SubscribeWithFilter<ClientTransferToMapMessage>(ClientTransferToMap, WorldId);
        }

        private void ClientCreateCharacterRequest(ClientCreateCharacterRequestMessage msg)
        {
            var characterResultMsg = new ClientCreateCharacterResultMessage();
            if (DatabaseService.CharacterNameExists(msg.Name))
            {
                characterResultMsg.Message = "Character name already exists";
                characterResultMsg.Success = false;
                WorldServer.SendMessageToClient(characterResultMsg, NetworkId);
            }
            else
            {
                if (!msg.Name.All(char.IsLetter))
                {
                    characterResultMsg.Message = "Invalid character name. May only consist of English letters - no spaces, symbols, or numbers";
                    characterResultMsg.Success = false;
                    WorldServer.SendMessageToClient(characterResultMsg, NetworkId);
                }
                else
                {
                    
                    if (!CharacterClassService.DoesClassExist(msg.Class) || !CharacterClassService.IsStartingClass(msg.Class))
                    {
                        characterResultMsg.Message = "Invalid character class";
                        characterResultMsg.Success = false;
                        WorldServer.SendMessageToClient(characterResultMsg, NetworkId);
                        return;
                    }

                    var playerClass = CharacterClassService.GetClassByName(msg.Class);
                    DatabaseService.AddCharacterName(msg.Name.ToLower());
                    var characters = UserDatabase.GetCollection<WorldCharacter>(WorldCharacter.TABLE);
                    UserDatabase.BeginTrans();
                    var characterPath = DatabaseService.CreateCharacterDatabase(msg.Name);
                    var characterDatabase = DatabaseService.OpenDatabase(characterPath);
                    var abilityCollection = characterDatabase.GetCollection<CharacterAbility>(CharacterAbility.TABLE);
                    var startingAbilities = AbilityService.GetStartingAbilities().ToList();
                    startingAbilities.AddRange(playerClass.GenerateStartingAbilities());
                    for (var i = 0; i < startingAbilities.Count; i++)
                    {
                        abilityCollection.Insert(startingAbilities[i].ToData());
                    }

                    var startingEquipment = playerClass.GenerateStartingEquipment();
                    var equipmentCollection = characterDatabase.GetCollection<CharacterEquippedItem>(CharacterEquippedItem.TABLE);
                    for (var i = 0; i < startingEquipment.Length; i++)
                    {
                        equipmentCollection.Insert(startingEquipment[i]);
                    }

                    var startingResources = playerClass.GeneratePlayerStartingResources();
                    var resourceCollection = characterDatabase.GetCollection<CharacterResource>(CharacterResource.TABLE);
                    for (var i = 0; i < startingResources.Length; i++)
                    {
                        resourceCollection.Insert(startingResources[i]);
                    }

                    var startingItems = playerClass.GeneratePlayerStartingItems();
                    var inventoryCollection = characterDatabase.GetCollection<WorldItem>(WorldItem.INVENTORY_TABLE);
                    for (var i = 0; i < startingItems.Length; i++)
                    {
                        inventoryCollection.Insert(startingItems[i]);
                    }

                    characterDatabase.Dispose();

                    
                    
                    var character = new WorldCharacter
                    {
                        Name = msg.Name,
                        UserId = UserId,
                        Map = MapService.DefaultMap.Name,
                        X = MapService.DefaultTile.Position.X,
                        Y = MapService.DefaultTile.Position.Y,
                        Level = 0,
                        Class = msg.Class,
                        Path = characterPath,
                        Sprite = playerClass.Sprites[0],
                        MaxInventorySlots = ItemService.StartingMaxInventorySlots,
                        Checkpoint = MapService.DefaultCheckpoint.Name
                    };
                    var id = characters.Insert(character);
                    var characterStatsCollection = UserDatabase.GetCollection<CharacterCombatStats>(CharacterCombatStats.TABLE);
                    var characterStats = new CharacterCombatStats(playerClass.StartingStats);
                    characterStats.CharacterId = id;
                    characterStats.CurrentHealth = characterStats.Health;
                    characterStatsCollection.Insert(characterStats);

                    var characterGrowthCollection = UserDatabase.GetCollection<CharacterGrowthStats>(CharacterGrowthStats.TABLE);
                    var characterGrowth = new CharacterGrowthStats();
                    characterGrowth.CharacterId = id;
                    characterGrowthCollection.Insert(characterGrowth);
                    UserDatabase.Commit();
                    characterResultMsg.Success = true;
                    WorldServer.SendMessageToClient(characterResultMsg, NetworkId);
                    _ticksSinceLastCheckIn = 0;
                }

            }

        }

        private void ClientCharacterRequest(ClientCharacterRequestMessage msg)
        {
            if (UserDatabase != null)
            {
                var characters = UserDatabase.GetCollection<WorldCharacter>(WorldCharacter.TABLE).FindAll().Select(c => c.ToData()).ToArray();
                WorldServer.SendMessageToClient(new ClientCharacterResultMessage{Characters = characters}, NetworkId);
            }
        }

        private void ClientEnterWorldWithCharacterRequest(ClientEnterWorldWithCharacterRequestMessage msg)
        {
            if (UserDatabase != null && Character == null)
            {
                var character = UserDatabase.GetCollection<WorldCharacter>(WorldCharacter.TABLE).FindOne(c => c.Name == msg.Name);
                if (character != null)
                {
                    var userCollection = DatabaseService.Main.GetCollection<WorldUser>(WorldUser.TABLE);
                    var user = userCollection.FindOne(u => u.Username == User);
                    user.ActiveCharacterId = character._id;
                    userCollection.Update(user);
                    var characterStats = UserDatabase.GetCollection<CharacterCombatStats>(CharacterCombatStats.TABLE).FindOne(c => c.CharacterId == character._id);
                    var characterGrowth = UserDatabase.GetCollection<CharacterGrowthStats>(CharacterGrowthStats.TABLE).FindOne(c => c.CharacterId == character._id);
                    var characterDatabase = DatabaseService.OpenDatabase(character.Path);

                    Character = ObjectManagerService.GeneratePlayerObject(character, characterStats, characterGrowth, characterDatabase, WorldId);
                    SubscribePlayerObjectMessages();

                    var enterWorldWithCharacterResultMsg = new ClientEnterWorldWithCharacterResultMessage {Success = true, GlobalCooldown = AbilityService.GlobalCooldown};
                    var queryCharacterDataMsg = new QueryPlayerCharacterDataMessage
                    {
                        DoAfter = data => { enterWorldWithCharacterResultMsg.Data = data; }
                    };
                    this.SendMessageTo(queryCharacterDataMsg, Character);
                    enterWorldWithCharacterResultMsg.Success = true;
                    enterWorldWithCharacterResultMsg.ObjectId = Character.Id;
                    WorldServer.SendMessageToClient(enterWorldWithCharacterResultMsg, NetworkId);
                    WorldServer.SendMessageToClient(new ClientCombatSettingsUpdateMessage{Settings = CombatService.CombatSettings}, NetworkId);

                    if (Character.Tile != null)
                    {
                        var objects = ObjectManagerService.GetClientObjectDataInArea(Character, false);
                        var blocking = MapService.GetBlockingTilesInAreaOfMap(Character.Tile.Position, Character.Map);
                        if (objects.Length > 0 || blocking.Length > 0)
                        {
                            WorldServer.SendMessageToClient(new ClientObjectUpdateMessage { Objects = objects, MapChange = false, Blocking = blocking }, NetworkId);
                        }
                    }
                }
                else
                {
                    WorldServer.SendMessageToClient(new ClientEnterWorldWithCharacterResultMessage{Success = false, Message = $"No character named {msg.Name}"}, NetworkId);
                }
            }
        }

        private void ClientCheckInResponse(ClientCheckInResponseMessage msg)
        {
            _ticksSinceLastCheckIn = 0;
        }

        private void WorldTick(WorldTickMessage msg)
        {
            //_ticksSinceLastCheckIn++;
            //if (_ticksSinceLastCheckIn >= ClientManagerService.DisconnectTicks)
            //{
            //    Disconnect();
            //    var unregisterClientMsg = new UnregisterClientMessage {NetworkId = NetworkId};
            //    this.SendMessage(unregisterClientMsg);
            //}
            //else if (_ticksSinceLastCheckIn >= ClientManagerService.CheckInTicks)
            //{
            //    WorldServer.SendMessageToClient(ClientCheckInRequestMessage.INSTANCE, NetworkId);
            //}
        }

        private void UpdateClientsTick(UpdateClientsTickMessage msg)
        {
            if (!_disconnecting && Character.Tile != null && !Character.BeingDestroy)
            {
                var objects = ObjectManagerService.GetClientObjectDataInArea(Character, false);
                var blockingTiles = MapService.GetBlockingTilesInAreaOfMap(Character.Tile.Position, Character.Map);
                if (objects.Length > 0 || blockingTiles.Length > 0)
                {
                    var clientObjectUpdateMessage = new ClientObjectUpdateMessage
                    {
                        Objects = objects,
                        Blocking = blockingTiles,
                        MapChange = false
                    };
                    WorldServer.SendMessageToClient(clientObjectUpdateMessage, NetworkId);
                }
                
                var worldEvents = MapService.GetEventsInAreaOnMap(Character.Map, Character.Tile);
                if (worldEvents.Length > 0)
                {
                    WorldServer.SendMessageToClient(new ClientWorldEventUpdateMessage{Events = worldEvents.Select(e => e.ToJson()).ToArray()}, NetworkId);
                }
            }
        }
        private void ClientMovementResponse(ClientMovementResponseMessage msg)
        {
            WorldServer.SendMessageToClient(msg, NetworkId);
        }

        private void ClientCharacterUpdate(ClientCharacterUpdateMessage msg)
        {
            WorldServer.SendMessageToClient(msg, NetworkId);
        }

        private void ClientUseAbilityResult(ClientUseAbilityResultMessage msg)
        {
            WorldServer.SendMessageToClient(msg, NetworkId);
        }

        private void CastCancelled(CastCancelledMessage msg)
        {
            WorldServer.SendMessageToClient(new ClientCastCancelledMessage(), NetworkId);
        }

        private void ClientCastFailed(ClientCastFailedMessage msg)
        {
            WorldServer.SendMessageToClient(msg, NetworkId);
        }

        private void ClientPlayerDead(ClientPlayerDeadMessage msg)
        {
            WorldServer.SendMessageToClient(msg, NetworkId);
        }

        private void ClientPlayerEventUpdate(ClientPlayerEventUpdateMessage msg)
        {
            WorldServer.SendMessageToClient(msg, NetworkId);
        }

        private void ClientBuyItemFromShopResult(ClientBuyItemFromShopResultMessage msg)
        {
            WorldServer.SendMessageToClient(msg, NetworkId);
        }

        private void ClientSellItemFromShopResult(ClientSellItemToShopResultMessage msg)
        {
            WorldServer.SendMessageToClient(msg, NetworkId);
        }

        private void ClientInteractWithObjectResult(ClientInteractWithObjectResultMessage msg)
        {
            WorldServer.SendMessageToClient(msg, NetworkId);
        }

        private void ClientFinishInteractionResult(ClientFinishInteractionResultMessage msg)
        {
            WorldServer.SendMessageToClient(msg, NetworkId);
        }

        private void ClientShowShop(ClientShowShopMessage msg)
        {
            WorldServer.SendMessageToClient(msg, NetworkId);
        }

        private void ClientShowLootWindow(ClientShowLootWindowMessage msg)
        {
            WorldServer.SendMessageToClient(msg, NetworkId);
        }

        private void ClientMoveItemToSlotResult(ClientMoveItemToSlotResultMessage msg)
        {
            WorldServer.SendMessageToClient(msg, NetworkId);
        }

        private void ClientTransferToMap(ClientTransferToMapMessage msg)
        {
            WorldServer.SendMessageToClient(msg, NetworkId);
            var objects = ObjectManagerService.GetClientObjectDataInArea(Character, false);
            var blockingTiles = MapService.GetBlockingTilesInAreaOfMap(Character.Tile.Position, Character.Map);
            if (objects.Length > 0 || blockingTiles.Length > 0)
            {
                var clientObjectUpdateMessage = new ClientObjectUpdateMessage
                {
                    Objects = objects,
                    Blocking = blockingTiles,
                    MapChange = true
                };
                WorldServer.SendMessageToClient(clientObjectUpdateMessage, NetworkId);
            }

            var worldEvents = MapService.GetEventsInAreaOnMap(Character.Map, Character.Tile);
            if (worldEvents.Length > 0)
            {
                WorldServer.SendMessageToClient(new ClientWorldEventUpdateMessage { Events = worldEvents.Select(e => e.ToJson()).ToArray() }, NetworkId);
            }
            WorldServer.SendMessageToClient(new ClientFinishMapTransferMessage(), NetworkId);
        }

        private void ClientPlayerRespawn(ClientPlayerRespawnMessage msg)
        {
            var objects = ObjectManagerService.GetClientObjectDataInArea(Character, false);
            var blocking = MapService.GetBlockingTilesInAreaOfMap(Character.Tile.Position, Character.Map);
            if (objects.Length > 0 || blocking.Length > 0)
            { 
                WorldServer.SendMessageToClient(new ClientObjectUpdateMessage { Objects = objects, MapChange = false, Blocking = blocking}, NetworkId);
            }

            var worldEvents = MapService.GetEventsInAreaOnMap(Character.Map, Character.Tile);
            if (worldEvents.Length > 0)
            {
                WorldServer.SendMessageToClient(new ClientWorldEventUpdateMessage { Events = worldEvents.Select(e => e.ToJson()).ToArray() }, NetworkId);
            }
            WorldServer.SendMessageToClient(msg, NetworkId);
        }

        

        private void SaveData(SaveDataMessage msg)
        {
            if (Character != null && UserDatabase != null)
            {
                var collection = UserDatabase.GetCollection<WorldCharacter>(WorldCharacter.TABLE);

                var character = collection.FindOne(c => c.Name == Character.DisplayName);
                if (character != null)
                {
                    var statsCollection = UserDatabase.GetCollection<CharacterCombatStats>(CharacterCombatStats.TABLE);
                    var growthCollection = UserDatabase.GetCollection<CharacterGrowthStats>(CharacterGrowthStats.TABLE);

                    this.SendMessageTo(new QueryGoldMessage{DoAfter = gold => { character.Gold = gold; }}, Character);
                    this.SendMessageTo(new QueryMaxInventorySlotsMessage{DoAfter = maxSlots => { character.MaxInventorySlots = maxSlots; }}, Character);

                    
                    var existingStats = statsCollection.FindOne(s => s.CharacterId == character._id);
                    var addStats = false;
                    if (existingStats == null)
                    {
                        existingStats = new CharacterCombatStats {CharacterId = character._id};
                        addStats = true;
                    }
                    this.SendMessageTo(new QueryCombatStatsMessage{DoAfter = (baseStats, bonusStats, currentHealth) =>
                    {
                        existingStats.FromCombatStats(baseStats,currentHealth);
                    }}, Character);
                    if (addStats)
                    {
                        statsCollection.Insert(existingStats);
                    }
                    else
                    {
                        statsCollection.Update(existingStats);
                    }
                    


                    var existingGrowth = growthCollection.FindOne(g => g.CharacterId == character._id);
                    var addGrowth = false;
                    if (existingGrowth == null)
                    {
                        existingGrowth = new CharacterGrowthStats{CharacterId = character._id};
                        addGrowth = true;
                    }
                    this.SendMessageTo(new QueryCombatGrowthStatsMessage{DoAfter = growthStats =>
                    {
                        existingGrowth.FromGrowth(growthStats);
                    }}, Character);
                    if (addGrowth)
                    {
                        growthCollection.Insert(existingGrowth);
                    }
                    else
                    {
                        growthCollection.Update(existingGrowth);
                    }

                    this.SendMessageTo(new QueryPlayerClassDataMessage{DoAfter = (playerClass, experience, level, unspentTalentPoints, talents) =>
                    {
                        character.Experience = experience;
                        character.Level = level;
                        character.UnspentTalentPoints = unspentTalentPoints;
                    }}, Character);

                    this.SendMessageTo(new QueryCurrentCheckpointMessage{DoAfter = checkPoint =>
                    {
                        character.Checkpoint = checkPoint.Name;
                    }}, Character);

                    character.X = Character.Tile.Position.X;
                    character.Y = Character.Tile.Position.Y;
                    character.Map = Character.Map;
                    //TODO: Update character info here
                    collection.Update(character);
                }
            }
        }

        private void DisconnectClient(DisconnectClientMessage msg)
        {
            _disconnecting = true;
            Session.SetAuthenticationState(false);
            this.Subscribe<ResolveTickMessage>(ResolveTick);
            //Disconnect();
        }

        private void ResolveTick(ResolveTickMessage msg)
        {
            if (_disconnecting)
            {
                Disconnect();
            }
        }


        public void Disconnect()
        {
            if (Character != null)
            {
                this.SendMessageTo(PlayerDisconnectingMessage.INSTANCE, Character);
                this.SendMessageTo(SaveDataMessage.INSTANCE, this);
                this.SendMessageTo(SaveDataMessage.INSTANCE, Character);
                ObjectManagerService.RemoveObjectFromWorld(Character.Map, Character);
                Character = null;
            }
            this.UnsubscribeFromAllMessages();
            UserDatabase?.Dispose();
            UserDatabase = null;

        }
    }
}