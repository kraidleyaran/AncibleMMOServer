using System.Collections.Generic;
using System.Linq;
using AncibleCoreCommon;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Client;
using AncibleCoreCommon.CommonData.WorldBonuses;
using AncibleCoreServer.Data;
using AncibleCoreServer.Services.CharacterClass;
using AncibleCoreServer.Services.ObjectManager;
using LiteDB;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class PlayerTrait : ObjectTrait
    {
        public const string TRAIT_NAME = "Player Trait";
        private string _playerId = string.Empty;

        private ClientCharacterData _characterData = null;
        private List<string> _playerEvents = new List<string>();

        private ILiteDatabase _characterDatabase = null;

        private bool _update = true;

        public PlayerTrait(string playerId, ILiteDatabase characterDatabase)
        {
            Name = TRAIT_NAME;
            _playerId = playerId;
            _characterDatabase = characterDatabase;
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            _characterData = new ClientCharacterData { Map = _parent.Map, Name = _parent.DisplayName, Sprite = _parent.Sprite, Position = _parent.Tile.Position};
            SubscribeToMessages();
        }

        private void SaveAbilities()
        {
            var playerAbilties = new List<ClientAbilityInfoData>();
            this.SendMessageTo(new QueryAbilitiesMessage
            {
                DoAfter = abilities => { playerAbilties = abilities.Select(a => a.GetClientData()).ToList(); }
            }, _parent);

            var abilityCollection = _characterDatabase.GetCollection<CharacterAbility>(CharacterAbility.TABLE);
            var existingAbilities = abilityCollection.FindAll().ToList();
            var removedAbilities = existingAbilities.Where(e => !playerAbilties.Exists(p => p.Name == e.Name)).ToList();
            existingAbilities = existingAbilities.Where(e => !removedAbilities.Contains(e)).ToList();
            for (var i = 0; i < removedAbilities.Count; i++)
            {
                abilityCollection.Delete(removedAbilities[i]._id);
            }
            for (var i = 0; i < playerAbilties.Count; i++)
            {
                var existing = existingAbilities.FirstOrDefault(e => e.Name == playerAbilties[i].Name);
                if (existing == null)
                {
                    abilityCollection.Insert(new CharacterAbility { Name = playerAbilties[i].Name, Rank = playerAbilties[i].Rank, OwnerMods = playerAbilties[i].OwnerMods, TargetMods = playerAbilties[i].TargetMods});
                }
                else
                {
                    existing.Rank = playerAbilties[i].Rank;
                    abilityCollection.Update(existing);
                }
            }
        }

        private void SaveInventory()
        {
            var items = new List<ClientItemData>();
            this.SendMessageTo(new QueryInventoryMessage{DoAfter = playerItems => items = playerItems.ToList()}, _parent);
            var inventoryCollection = _characterDatabase.GetCollection<WorldItem>(WorldItem.INVENTORY_TABLE);
            var existingItems = inventoryCollection.FindAll().ToList();
            var removedItems = existingItems.Where(i => !items.Exists(it => it.Slot == i.Slot)).ToArray();
            for (var i = 0; i < removedItems.Length; i++)
            {
                existingItems.Remove(removedItems[i]);
                inventoryCollection.Delete(removedItems[i]._id);
            }

            for (var i = 0; i < items.Count; i++)
            {
                var existing = existingItems.FirstOrDefault(e => e.Slot == items[i].Slot);
                if (existing == null)
                {
                    inventoryCollection.Insert(new WorldItem(items[i]));
                }
                else
                {
                    existing.FromData(items[i]);
                    inventoryCollection.Update(existing);
                }
            }
        }

        private void SaveEquipment()
        {
            var equippedItems = new List<ClientEquippedItemData>();
            this.SendMessageTo(new QueryEquipmentMessage{DoAfter = playerEquipment=> equippedItems = playerEquipment.ToList()}, _parent);

            var equipmentCollection = _characterDatabase.GetCollection<CharacterEquippedItem>(CharacterEquippedItem.TABLE);
            var existingItems = equipmentCollection.FindAll().ToList();
            var removedItems = existingItems.Where(e => !equippedItems.Exists(i => e.Slot == i.Slot)).ToArray();
            for (var i = 0; i < removedItems.Length; i++)
            {
                existingItems.Remove(removedItems[i]);
                equipmentCollection.Delete(removedItems[i]._id);
            }

            for (var i = 0; i < equippedItems.Count; i++)
            {
                var existing = existingItems.FirstOrDefault(e => e.Slot == equippedItems[i].Slot);
                if (existing == null)
                {
                    equipmentCollection.Insert(new CharacterEquippedItem(equippedItems[i]));
                }
                else
                {
                    existing.FromClientData(equippedItems[i]);
                    equipmentCollection.Update(existing);
                }
            }
        }

        private void SaveResources()
        {
            var playerResources = new List<ClientResourceData>();
            this.SendMessageTo(new QueryResourceMessage{DoAfter = resouruces => playerResources = resouruces.ToList()}, _parent);
            var resourceCollection = _characterDatabase.GetCollection<CharacterResource>(CharacterResource.TABLE);
            var existingResources = resourceCollection.FindAll().ToList();
            var removedResources = existingResources.FindAll(r => !playerResources.Exists(e => r.Resource == e.Resource)).ToArray();
            for (var i = 0; i < removedResources.Length; i++)
            {
                existingResources.Remove(removedResources[i]);
                resourceCollection.Delete(removedResources[i]._id);
            }

            for (var i = 0; i < playerResources.Count; i++)
            {
                var existing = existingResources.FirstOrDefault(r => r.Resource == playerResources[i].Resource);
                if (existing == null)
                {
                    resourceCollection.Insert(new CharacterResource(playerResources[i]));
                }
                else
                {
                    existing.FromClientData(playerResources[i]);
                    resourceCollection.Update(existing);
                }
            }
        }

        private void SaveTalents()
        {
            var characterTalents = new List<ClientTalentData>();
            this.SendMessageTo(new QueryPlayerClassDataMessage{DoAfter =
                (playerClass, experience, level, unspentTalentPoints, talents) =>
                {
                    characterTalents = talents.ToList();
                }}, _parent);
            var talentCollection = _characterDatabase.GetCollection<CharacterTalent>(CharacterTalent.TABLE);
            var existingTalents = talentCollection.FindAll().ToList();
            var deleteTalents = existingTalents.Where(t => !characterTalents.Exists(e => e.Name == t.Name)).ToArray();
            for (var i = 0; i < deleteTalents.Length; i++)
            {
                existingTalents.Remove(deleteTalents[i]);
                talentCollection.Delete(deleteTalents[i]._id);
            }

            for (var i = 0; i < characterTalents.Count; i++)
            {
                var existing = existingTalents.FirstOrDefault(e => e.Name == characterTalents[i].Name);
                if (existing == null)
                {
                    talentCollection.Insert(new CharacterTalent(characterTalents[i]));
                }
                else if (existing.Rank != characterTalents[i].Rank)
                {
                    existing.Rank = characterTalents[i].Rank;
                    talentCollection.Update(existing);
                }
            }
        }

        private void SaveBonuses()
        {
            var worldBonuses = new WorldBonusData[0];
            this.SendMessageTo(new QueryWorldBonusesMessage{DoAfter = bonuses => worldBonuses = bonuses}, _parent);
            var bonusesCollection = _characterDatabase.GetCollection<CharacterWorldBonus>(CharacterWorldBonus.TABLE);
            bonusesCollection.DeleteAll();
            for (var i = 0; i < worldBonuses.Length; i++)
            {
                bonusesCollection.Insert(new CharacterWorldBonus(worldBonuses[i]));
            }
        }

        private void SubscribeToMessages()
        {
            this.Subscribe<UpdateClientsTickMessage>(UpdateClientsTick);
            this.Subscribe<SaveDataMessage>(SaveData);
            this.Subscribe<ResolveTickMessage>(ResolveTick);
            this.SubscribeWithFilter<ClientDeathConfirmationMessage>(ClientDeathConfirmation, _playerId);

            _parent.SubscribeWithFilter<QueryPlayerCharacterDataMessage>(QueryPlayerCharacterData, _instanceId);
            _parent.SubscribeWithFilter<FlagPlayerForUpdateMessage>(FlagPlayerForUpdate, _instanceId);
            _parent.SubscribeWithFilter<UpdateObjectStateMessage>(UpdateObjectState, _instanceId);
            _parent.SubscribeWithFilter<RegisterPlayerEventMessage>(RegisterPlayerEvent, _instanceId);
        }

        private void QueryPlayerCharacterData(QueryPlayerCharacterDataMessage msg)
        {
            _characterData.Position = _parent.Tile.Position;
            _characterData.Sprite = _parent.Sprite;
            _characterData.Map = _parent.Map;
            this.SendMessageTo(new QueryAbilitiesMessage
            {
                DoAfter = abilities =>
                {
                    _characterData.Abilities = abilities.Select(a => a.GetClientData()).ToArray();
                }
            }, _parent);

            this.SendMessageTo(new QueryCombatStatsMessage{DoAfter = (baseStats, bonusStats, currentHealth) =>
            {
                _characterData.BaseStats = baseStats;
                _characterData.BonusStats = bonusStats;
                _characterData.CurrentHealth = currentHealth;
            }}, _parent);

            this.SendMessageTo(new QueryInventoryMessage{DoAfter = inventory =>
            {
                _characterData.Inventory = inventory;
            }}, _parent);

            this.SendMessageTo(new QueryMaxInventorySlotsMessage{DoAfter = slots =>
            {
                _characterData.MaxInventorySlots = slots;
            }}, _parent);

            this.SendMessageTo(new QueryResourceMessage
            {
                DoAfter = resources =>
                {
                    _characterData.Resources = resources;
                }
            }, _parent);

            var statusEffects = new List<ClientStatusEffectData>();
            this.SendMessageTo(new QueryClientStatusEffectsMessage{DoAfter = effects =>
            {
                statusEffects.Add(effects);
            }}, _parent);
            _characterData.StatusEffects = statusEffects.ToArray();

            this.SendMessageTo(new QueryPlayerClassDataMessage{DoAfter = (playerClass, experience, level, unspentTalentPoints, talents) =>
                {
                    _characterData.PlayerClass = playerClass;
                    _characterData.Experience = experience;
                    _characterData.Level = level;
                    _characterData.NextLevelExperience = CharacterClassService.GetLevelExperience(level);
                    _characterData.Talents = talents;
                    _characterData.UnspentTalentPoints = unspentTalentPoints;
                }}, _parent);

            msg.DoAfter.Invoke(_characterData);
        }

        private void UpdateClientsTick(UpdateClientsTickMessage msg)
        {
            if (_update)
            {
                _update = false;
                _characterData.Position = _parent.Tile.Position;
                _characterData.Sprite = _parent.Sprite;
                _characterData.Map = _parent.Map;

                this.SendMessageTo(new QueryAbilitiesMessage{DoAfter = abilities =>
                {
                    _characterData.Abilities = abilities.Select(a => a.GetClientData()).ToArray();
                }}, _parent);

                this.SendMessageTo(new QueryInventoryMessage{DoAfter = items =>
                {
                    _characterData.Inventory = items;
                }}, _parent);

                this.SendMessageTo(new QueryEquipmentMessage{DoAfter = equipment =>
                {
                    _characterData.Equipment = equipment;
                }}, _parent);

                this.SendMessageTo(new QueryResourceMessage{DoAfter = resources =>
                {
                    _characterData.Resources = resources;
                }}, _parent);

                this.SendMessageTo(new QueryCombatStatsMessage
                {
                    DoAfter = (baseStats, bonusStats, currentHealth) =>
                    {
                        _characterData.BaseStats = baseStats;
                        _characterData.BonusStats = bonusStats;
                        _characterData.CurrentHealth = currentHealth;
                    }
                }, _parent);

                this.SendMessageTo(new QueryPlayerClassDataMessage
                {
                    DoAfter = (playerClass, experience, level, unspentTalentPoints, talents) =>
                    {
                        _characterData.PlayerClass = playerClass;
                        _characterData.Experience = experience;
                        _characterData.Level = level;
                        _characterData.NextLevelExperience = CharacterClassService.GetLevelExperience(_characterData.Level);
                        _characterData.Talents = talents;
                        _characterData.UnspentTalentPoints = unspentTalentPoints;
                    }
                }, _parent);

                var weaponDamage = new IntNumberRange(0, 0);
                this.SendMessageTo(new QueryWeaponDamageMessage{DoAfter = damage => weaponDamage = damage}, _parent);
                _characterData.WeaponDamage = weaponDamage;

                var icons = new List<ClientObjectIconData>();
                this.SendMessageTo(new QueryClientIconDataMessage{DoAfter = icon => icons.Add(icon)}, _parent );
                _characterData.Icons = icons.ToArray();

                this.SendMessageTo(new QueryGoldMessage{DoAfter = gold => { _characterData.Gold = gold; }}, _parent);
                this.SendMessageTo(new ClientCharacterUpdateMessage{Data = _characterData}, _parent);
                this.SendMessageTo(new ClientPlayerEventUpdateMessage{Events = _playerEvents.ToArray()}, _parent);
            }
        }

        private void FlagPlayerForUpdate(FlagPlayerForUpdateMessage msg)
        {
            _update = true;
        }

        private void SaveData(SaveDataMessage msg)
        {
            SaveAbilities();
            SaveInventory();
            SaveEquipment();
            SaveResources();
            SaveTalents();
            SaveBonuses();
        }

        private void ClientDeathConfirmation(ClientDeathConfirmationMessage msg)
        {
            this.SendMessageTo(RespawnPlayerMessage.INSTANCE, _parent);
            this.SendMessageTo(new SetObjectStateMessage{State = ObjectState.Active}, _parent);
            this.SendMessageTo(new ClientPlayerRespawnMessage{Map = _parent.Map, Tile = _parent.Tile.Position}, _parent);
            
        }

        private void UpdateObjectState(UpdateObjectStateMessage msg)
        {
            if (msg.State == ObjectState.Dead)
            {
                this.SendMessageTo(new ClientPlayerDeadMessage(), _parent);
            }
        }

        private void ResolveTick(ResolveTickMessage msg)
        {
            _playerEvents.Clear();
        }

        private void RegisterPlayerEvent(RegisterPlayerEventMessage msg)
        {
            var json = AncibleUtils.ConverToJson(msg.Event);
            _playerEvents.Add(json);
        }

        public override void Destroy()
        {
            _characterDatabase.Dispose();
            this.UnsubscribeFromAllMessagesWithFilter(_playerId);
            base.Destroy();
        }
    }
}