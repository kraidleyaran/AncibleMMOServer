using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Client;
using AncibleCoreCommon.CommonData.Items;
using AncibleCoreCommon.CommonData.Maps;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Data;
using AncibleCoreServer.Services.Maps;
using AncibleCoreServer.Services.Traits;
using FileDataLib;
using LiteDB;
using MessageBusLib;

namespace AncibleCoreServer.Services.ObjectManager
{
    public class ObjectManagerService : WorldService
    {
        public static List<string> PLAYER_SPRITES { get; private set; }
        public static int DefaultInteractionRange { get; private set; }

        public override string Name => "Object Manager Service";

        private static ObjectManagerService _instance = null;
        

        private Dictionary<string, WorldObject> _allObjects = new Dictionary<string, WorldObject>();

        private string _objectSpawnPath = string.Empty;

        public ObjectManagerService(string objectSpawnPath)
        {
            PLAYER_SPRITES = new List<string>();
            _objectSpawnPath = objectSpawnPath;
        }

        public override void Start()
        {
            if (_instance == null)
            {
                _instance = this;
                if (Directory.Exists(_objectSpawnPath))
                {
                    var files = Directory.GetFiles(_objectSpawnPath, $"*.{DataExtensions.OBJECT_SPAWN}");
                    var generatedMax = 0;
                    var successCount = 0;
                    for (var i = 0; i < files.Length; i++)
                    {
                        var response = FileData.LoadData<MapSpawnData>(files[i]);
                        if (response.Success)
                        {
                            generatedMax += response.Data.Spawns.Length;
                            for (var s = 0; s < response.Data.Spawns.Length; s++)
                            {
                                if (GenerateObjectFromSpawn(response.Data.Map, response.Data.Spawns[s]))
                                {
                                    successCount++;
                                }
                            }
                        }
                    }
                    Log($"Loaded {successCount} out of {generatedMax} Object Spawns");
                }
                base.Start();
                SubscribeToMessages();
            }

        }

        public static void RemoveObjectFromWorld(string map, WorldObject obj)
        {
            
            _instance._allObjects.Remove(obj.Id);
            obj.Destroy();
        }

        public static WorldObject GetObjectFromId(string id)
        {
            if (_instance._allObjects.TryGetValue(id, out var obj))
            {
                return obj;
            }

            return null;
        }

        public static ClientObjectData[] GetClientObjectDataInArea(WorldObject obj, bool updateOnly = true)
        {
            var objectData = new List<ClientObjectData>();
            var tiles = MapService.GetMapTilesInRectangleArea(obj.Map, obj.Tile, MapService.CullingBox.X, MapService.CullingBox.Y).Where(t => t.ObjectsOnTile.Count > 0 && t.ObjectsOnTile.Exists(o => o.Visible)).ToArray();
            
            if (updateOnly)
            {
                tiles = tiles.Where(t => t.ObjectsOnTile.Exists(o => o.Update)).ToArray();
            }
            
            for (var i = 0; i < tiles.Length; i++)
            {
                objectData.AddRange(tiles[i].ObjectsOnTile.Where(o => !o.BeingDestroy && o.Visible && (o.VisibleList.Count <= 0 || o.VisibleList.Contains(obj.Id))).Select(o => o.GetClientObjectData()));
            }

            return objectData.ToArray();
        }

        public static WorldObject GeneratePlayerObject(WorldCharacter character, CharacterCombatStats stats, CharacterGrowthStats growthStats, ILiteDatabase characterDatabase,string playerId)
        {
            var obj = new WorldObject();
            var tile = MapService.GetMapTileInMapByPosition(character.Map, new Vector2IntData(character.X, character.Y));
            if (tile == null)
            {
                obj.Tile = MapService.DefaultTile;
                obj.Map = MapService.DefaultMap.Name;
            }
            else
            {
                obj.Tile = tile;
                obj.Map = character.Map;
            }
            obj.Tile.ObjectsOnTile.Add(obj);
            obj.DisplayName = character.Name;
            obj.Visible = true;
            var addTraitToObjectMsg = new AddTraitToObjectMessage();
            var spriteTrait = TraitService.GetSpriteTrait(character.Sprite);
            if (spriteTrait != null)
            {
                addTraitToObjectMsg.Trait = spriteTrait;
                _instance.SendMessageTo(addTraitToObjectMsg, obj);
            }

            addTraitToObjectMsg.Trait = new ObjectStateTrait();
            _instance.SendMessageTo(addTraitToObjectMsg, obj);

            addTraitToObjectMsg.Trait = new CombatTrait(CombatAlignment.Player);
            _instance.SendMessageTo(addTraitToObjectMsg, obj);

            addTraitToObjectMsg.Trait = new PlayerCombatStatsTrait(stats, growthStats);
            _instance.SendMessageTo(addTraitToObjectMsg, obj);

            var abilities = characterDatabase.GetCollection<CharacterAbility>(CharacterAbility.TABLE).FindAll().ToArray();
            addTraitToObjectMsg.Trait = new PlayerAbilityManagerTrait(playerId, abilities);
            _instance.SendMessageTo(addTraitToObjectMsg, obj);

            var items = characterDatabase.GetCollection<WorldItem>(WorldItem.INVENTORY_TABLE).FindAll().ToArray();
            addTraitToObjectMsg.Trait = new PlayerInventoryTrait(playerId, items, character.MaxInventorySlots, character.Gold);
            _instance.SendMessageTo(addTraitToObjectMsg, obj);

            var resources = characterDatabase.GetCollection<CharacterResource>(CharacterResource.TABLE).FindAll().ToArray();
            addTraitToObjectMsg.Trait = new PlayerResourceManagerTrait(resources);
            _instance.SendMessageTo(addTraitToObjectMsg, obj);

            addTraitToObjectMsg.Trait = new PlayerInteractionTrait(playerId);
            _instance.SendMessageTo(addTraitToObjectMsg, obj);

            addTraitToObjectMsg.Trait = new PlayerEquipmentTrait(playerId);
            _instance.SendMessageTo(addTraitToObjectMsg, obj);

            addTraitToObjectMsg.Trait = new CastingTrait();
            _instance.SendMessageTo(addTraitToObjectMsg, obj);

            var checkpoint = MapService.GetCheckpointByName(character.Checkpoint) ?? MapService.DefaultCheckpoint;
            addTraitToObjectMsg.Trait = new PlayerMovementTrait(playerId, checkpoint, 1);
            _instance.SendMessageTo(addTraitToObjectMsg, obj);

            var talents = characterDatabase.GetCollection<CharacterTalent>(CharacterTalent.TABLE).FindAll().ToArray();
            addTraitToObjectMsg.Trait = new PlayerClassTrait(character.Class, talents, character.Level, character.Experience, playerId);
            _instance.SendMessageTo(addTraitToObjectMsg, obj);

            var bonuses = characterDatabase.GetCollection<CharacterWorldBonus>(CharacterWorldBonus.TABLE).FindAll().ToArray();
            addTraitToObjectMsg.Trait = new PlayerBonusManagerTrait(bonuses);
            _instance.SendMessageTo(addTraitToObjectMsg, bonuses);

            addTraitToObjectMsg.Trait = new PlayerTrait(playerId, characterDatabase);
            _instance.SendMessageTo(addTraitToObjectMsg, obj);

            var equipment = characterDatabase.GetCollection<CharacterEquippedItem>(CharacterEquippedItem.TABLE).FindAll().ToArray();
            _instance.SendMessageTo(new SetCharacterEquipmentMessage{Equipment = equipment}, obj);
            
            
            _instance._allObjects.Add(obj.Id, obj);

            return obj;

        }

        private bool GenerateObjectFromSpawn(string map, ObjectSpawnData data)
        {
            if (MapService.DoesMapExist(map))
            {
                var tile = MapService.GetMapTileInMapByPosition(map, data.Position);
                if (tile != null)
                {
                    var obj = new WorldObject
                    {
                        Tile = tile,
                        Map = map,
                        Visible = data.Visible,
                        DisplayName = data.Name
                    };
                    obj.Tile.ObjectsOnTile.Add(obj);
                    if (data.Blocking)
                    {
                        MapService.SetObstacleOnMapTile(obj, map, data.Position);
                    }
                    var traits = data.Traits.Select(TraitService.GetTrait).Where(t => t != null).ToArray();
                    var addTraitToObjMsg = new AddTraitToObjectMessage();
                    for (var i = 0; i < traits.Length; i++)
                    {
                        addTraitToObjMsg.Trait = traits[i];
                        this.SendMessageTo(addTraitToObjMsg, obj);
                    }
                    _instance.SendMessageTo(RefreshObjectStateMessage.INSTANCE, obj);
                    _instance._allObjects.Add(obj.Id, obj);
                    return true;
                }

                return false;

            }

            return false;

        }

        public static WorldObject GenerateObjectFromTemplate(ObjectTemplateData template, Vector2IntData position, string map)
        {
            if (MapService.DoesMapExist(map))
            {
                var tile = MapService.GetMapTileInMapByPosition(map, position);
                if (tile != null)
                {
                    var obj = new WorldObject
                    {
                        DisplayName = template.ObjectName,
                        Map = map,
                        Tile = tile,
                        Visible = true,
                    };

                    obj.Tile.ObjectsOnTile.Add(obj);

                    var traits = template.Traits.Where(TraitService.DoesTraitExist).Select(TraitService.GetTrait).ToArray();
                    if (traits.Length > 0)
                    {
                        var addTraitToObjMsg = new AddTraitToObjectMessage();
                        for (var i = 0; i < traits.Length; i++)
                        {
                            addTraitToObjMsg.Trait = traits[i];
                            _instance.SendMessageTo(addTraitToObjMsg, obj);
                        }
                    }

                    _instance.SendMessageTo(RefreshObjectStateMessage.INSTANCE, obj);
                    _instance._allObjects.Add(obj.Id, obj);

                    return obj;
                }

                return null;
            }

            return null;
        }

        public static WorldObject GenerateObjectFromTraits(string[] traits, string name, Vector2IntData pos, string map)
        {
            if (MapService.DoesMapExist(map))
            {
                var tile = MapService.GetMapTileInMapByPosition(map, pos);
                if (tile != null)
                {
                    var obj = new WorldObject
                    {
                        DisplayName = name,
                        Map = map,
                        Tile = tile,
                        Visible = true,
                    };

                    obj.Tile.ObjectsOnTile.Add(obj);

                    var objTraits = traits.Where(TraitService.DoesTraitExist).Select(TraitService.GetTrait).ToArray();
                    if (objTraits.Length > 0)
                    {
                        var addTraitToObjMsg = new AddTraitToObjectMessage();
                        for (var i = 0; i < objTraits.Length; i++)
                        {
                            addTraitToObjMsg.Trait = objTraits[i];
                            _instance.SendMessageTo(addTraitToObjMsg, obj);
                        }
                    }
                    _instance._allObjects.Add(obj.Id, obj);
                    return obj;
                }

                return null;
            }

            return null;
        }

        public static WorldObject GenerateLootObject(LootTableData lootTable, string map, Vector2IntData pos, WorldObject[] looters)
        {
            var items = lootTable.GenerateItems();
            var gold = lootTable.Gold.GenerateRandomNumber(RNGService.RANDOM);
            if (items.Length > 0 || gold > 0)
            {
                var tile = MapService.GetMapTileInMapByPosition(map, pos);
                if (tile != null)
                {
                    var spriteTrait = TraitService.GetTrait(lootTable.Sprite);
                    if (spriteTrait != null)
                    {
                        var obj = new WorldObject { DisplayName = lootTable.DisplayName, Tile = tile, Map = map, Visible = true};
                        obj.Tile.ObjectsOnTile.Add(obj);
                        var addTraitToObjMsg = new AddTraitToObjectMessage {Trait = spriteTrait};
                        _instance.SendMessageTo(addTraitToObjMsg, obj);

                        addTraitToObjMsg.Trait = new LootableTrait(items, gold, looters.ToList());
                        _instance.SendMessageTo(addTraitToObjMsg, obj);

                        addTraitToObjMsg.Trait = new CombatTrait(CombatAlignment.Object);
                        _instance.SendMessageTo(addTraitToObjMsg, obj);

                        _instance._allObjects.Add(obj.Id, obj);

                        return obj;
                    }
                    else
                    {
                        return null;
                    }
                    
                }
                else
                {
                    return null;
                }
                
            }
            else
            {
                return null;
            }
        }

        public static WorldObject GenerateObject(string map, Vector2IntData pos, string name)
        {
            var tile = MapService.GetMapTileInMapByPosition(name, pos);
            if (tile != null)
            {
                var obj = new WorldObject {Map = map, Tile = tile, Visible = false, DisplayName = name};
                _instance._allObjects.Add(obj.Id, obj);
                return obj;
            }

            return null;
        }

        private void SubscribeToMessages()
        {
            this.Subscribe<ResolveTickMessage>(ResolveTick);
            this.Subscribe<SetDefaultInteractionRangeMessage>(SetDefaultInteractionRange);
        }

        private void ResolveTick(ResolveTickMessage msg)
        {
            var updatedObjects = _allObjects.Values.ToList().FindAll(o => o.Update);
            for (var i = 0; i < updatedObjects.Count; i++)
            {
                updatedObjects[i].Update = false;
            }
        }

        private void SetDefaultInteractionRange(SetDefaultInteractionRangeMessage msg)
        {
            DefaultInteractionRange = msg.Range;
            Log($"Default Interaction Range set to {DefaultInteractionRange}");
        }
        
    }
}