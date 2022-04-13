using System;
using System.Collections.Generic;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Ability;
using AncibleCoreCommon.CommonData.Client;
using AncibleCoreCommon.CommonData.Combat;
using AncibleCoreCommon.CommonData.Items;
using AncibleCoreCommon.CommonData.PlayerEvent;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreCommon.CommonData.WorldBonuses;
using AncibleCoreServer.Data;
using AncibleCoreServer.Services.Ability;
using AncibleCoreServer.Services.Maps;
using AncibleCoreServer.Services.ObjectManager;
using AncibleCoreServer.Services.Traits;
using MessageBusLib;

namespace AncibleCoreServer
{
    public class RegisterClientMessage : EventMessage
    {
        public int NetworkId;
    }

    public class UnregisterClientMessage : EventMessage
    {
        public int NetworkId;
    }

    public class ExecuteServerCommandMessage : EventMessage
    {
        public string Command;
        public string[] Arguments;
    }

    public class CreateUserMessage : EventMessage
    {
        public string Username;
        public string Password;
    }

    public class AddTraitToObjectMessage : EventMessage
    {
        public ObjectTrait Trait;
    }

    public class RemoveTraitFromObjectMessage : EventMessage
    {
        public ObjectTrait Trait;
    }

    public class WorldTickMessage : EventMessage
    {
        public static WorldTickMessage INSTANCE = new WorldTickMessage();
    }

    public class SetDefaultPlayerCharacterTraitsMessage : EventMessage
    {
        public string[] Traits;
    }

    public class QueryClientObjectDataMessage : EventMessage
    {
        public ClientObjectData Data;
    }

    public class UpdateClientsTickMessage : EventMessage
    {
        public static UpdateClientsTickMessage INSTANCE = new UpdateClientsTickMessage();
    }

    public class ResolveTickMessage : EventMessage
    {
        public static ResolveTickMessage INSTANCE = new ResolveTickMessage();
    }

    public class QueryClientIdMessage : EventMessage
    {
        public Action<string> DoAfter;
    }

    public class SaveDataMessage : EventMessage
    {
        public static SaveDataMessage INSTANCE = new SaveDataMessage();
    }

    public class BattleResultMessage : EventMessage
    {
        public bool Victory;
        public string EncounterId;
        public int Experience;
        public bool Loot;
        public bool Flee;
        public int TurnCount;
        public int DamageDone;
        public int HealingDone;
        public int DamageTaken;
    }

    public class RefreshPlayerDataMessage : EventMessage
    {
        public static RefreshPlayerDataMessage INSTANCE = new RefreshPlayerDataMessage();
    }

    public class PlayerDisconnectingMessage : EventMessage
    {
        public static PlayerDisconnectingMessage INSTANCE = new PlayerDisconnectingMessage();
    }

    public class UserListMessage : EventMessage
    {
        public static UserListMessage INSTANCE = new UserListMessage();
    }

    public class WorldSaveMessage : EventMessage
    {
        public static WorldSaveMessage INSTANCE = new WorldSaveMessage();
    }

    public class DisconnectClientMessage : EventMessage
    {
        public static DisconnectClientMessage INSTANCE = new DisconnectClientMessage();
    }

    public class ResetPasswordForUserMessage : EventMessage
    {
        public string User;
    }

    public class SetDefaultTileMessage : EventMessage
    {
        public string Map;
        public Vector2IntData Position;
    }

    public class SetDefaultPlayerSpriteMessage : EventMessage
    {
        public string Trait;
    }

    public class SetStartingClassesMessage : EventMessage
    {
        public string[] Classes;
    }

    public class QueryPlayerCharacterDataMessage : EventMessage
    {
        public Action<ClientCharacterData> DoAfter;
    }

    public class SetObjectStateMessage : EventMessage
    {
        public ObjectState State;
    }

    public class UpdateObjectStateMessage : EventMessage
    {
        public ObjectState State;
    }

    public class QueryObjectStateMessage : EventMessage
    {
        public Action<ObjectState> DoAfter;
    }

    public class UpdateTileMessage : EventMessage
    {
        public MapTile Tile;
    }


    public class SetAiStateMessage : EventMessage
    {
        public AiState State;
    }

    public class UpdateAiStateMessage : EventMessage
    {
        public AiState State;
    }

    public class QueryAiStateMessage : EventMessage
    {
        public Action<AiState> DoAfter;
    }

    public class SetDirectionMessage : EventMessage
    {
        public Vector2IntData Direction;
    }

    public class QueryCombatAlignmentMessage : EventMessage
    {
        public Action<CombatAlignment> DoAfter;
    }

    public class RefreshObjectStateMessage : EventMessage
    {
        public static RefreshObjectStateMessage INSTANCE = new RefreshObjectStateMessage();
    }

    public class UpdateCombatAlignmentMessage : EventMessage
    {
        public CombatAlignment Alignment;
    }

    public class AddAbilitiyMessage : EventMessage
    {
        public AbilityData Ability;
    }

    public class RemoveAbilityMessage : EventMessage
    {
        public string Ability;
    }

    public class QueryAbilitiesMessage : EventMessage
    {
        public Action<Ability[]> DoAfter;
    }

    public class FlagPlayerForUpdateMessage : EventMessage
    {
        public static FlagPlayerForUpdateMessage INSTANCE = new FlagPlayerForUpdateMessage();
    }

    public class SetStartingAbilitiesMessage : EventMessage
    {
        public string[] Abilities;
    }

    public class TakeDamageMessage : EventMessage
    {
        public int Amount;
        public DamageType Type;
        public WorldObject Owner;
    }

    public class CastCommandMessage : EventMessage
    {
        public string Name;
        public int Time;
        public Action DoAfter;
        public int Loop;
        public Action<bool> OnSuccess;
    }

    public class QueryCombatStatsMessage : EventMessage
    {
        public Action<CombatStats, CombatStats, int> DoAfter;
    }

    public class QueryCombatGrowthStatsMessage : EventMessage
    {
        public Action<CombatGrowthStats> DoAfter;
    }

    public class AbilityCheckMessage : EventMessage
    {
        public WorldObject Target;
        public Action<bool> OnAbilityUse;
    }

    public class QueryWorldObjectMessage : EventMessage
    {
        public Action<WorldObject> DoAfter;
    }

    public class CastCancelledMessage : EventMessage
    {
        public static CastCancelledMessage INSTANCE = new CastCancelledMessage();
    }

    public class ObjectDeadMessage : EventMessage
    {
        public WorldObject Object;
    }

    public class RespawnPlayerMessage : EventMessage
    {
        public static RespawnPlayerMessage INSTANCE = new RespawnPlayerMessage();
    }

    public class CancelCastMessage : EventMessage
    {
        public static CancelCastMessage INSTANCE = new CancelCastMessage();
    }

    public class SetGlobalCooldownMessage : EventMessage
    {
        public int Ticks;
    }

    public class AddItemToInventoryMessage : EventMessage
    {
        public ItemData Item;
        public int Stack;
        public Action<int> ReturnStack;
        public bool Announce;
    }

    public class RemoveItemFromInventoryByNameMessage : EventMessage
    {
        public string Item;
        public int Stack;
    }

    public class QueryInventoryMessage : EventMessage
    {
        public Action<ClientItemData[]> DoAfter;
    }

    public class QueryInventoryItemsByNameMessage : EventMessage
    {
        public string[] Items;
        public Action<ClientItemData[]> DoAfter;
    }

    public class SetStartingMaxInventorySlotsMessage : EventMessage
    {
        public int Max;
    }

    public class AddGoldMessage : EventMessage
    {
        public int Amount;
    }

    public class RemoveGoldMessage : EventMessage
    {
        public int Amount;
    }

    public class QueryGoldMessage : EventMessage
    {
        public Action<int> DoAfter;
    }

    public class QueryMaxInventorySlotsMessage : EventMessage
    {
        public Action<int> DoAfter;
    }

    public class AddLooterMessage : EventMessage
    {
        public WorldObject Obj;
    }

    public class SpawnLootMessage : EventMessage
    {
        public static SpawnLootMessage INSTANCE = new SpawnLootMessage();
    }

    public class HealMessage : EventMessage
    {
        public int Amount;
        public WorldObject Owner;
        public bool Broadcast;
    }

    public class RegisterPlayerEventMessage : EventMessage
    {
        public PlayerEvent Event;
    }

    public class EquipItemMessage : EventMessage
    {
        public ClientItemData Item;
    }

    public class AddItemByIdMessage : EventMessage
    {
        public ItemData Data;
        public string ItemId;
    }

    public class QueryEquipmentMessage : EventMessage
    {
        public Action<ClientEquippedItemData[]> DoAfter;
    }

    public class QueryAvailableInventorySlotsMessage : EventMessage
    {
        public Action<int> DoAfter;
    }

    public class SetCharacterEquipmentMessage : EventMessage
    {
        public CharacterEquippedItem[] Equipment;
    }

    public class ApplyCombatStatsMessage : EventMessage
    {
        public CombatStats Stats;
        public bool Permanent;
    }

    public class RemoveCombatStatsMessage : EventMessage
    {
        public CombatStats Stats;
        public bool Permanent;
    }

    public class QueryWeaponDamageMessage : EventMessage
    {
        public Action<IntNumberRange> DoAfter;
    }

    public class AddResourceMessage : EventMessage
    {
        public ResourceType Type;
        public int Amount;
    }

    public class RemoveResourceMessage : EventMessage
    {
        public ResourceType Type;
        public int Amount;
    }

    public class QueryResourceMessage : EventMessage
    {
        public Action<ClientResourceData[]> DoAfter;
    }

    public class QueryResourceByTypeMessage : EventMessage
    {
        public ResourceType Type;
        public Action<ClientResourceData> DoAfter;
    }

    public class ApplyMaximumResourceMessage : EventMessage
    {
        public ResourceType Type;
        public int Amount;
        public bool Permanent;
    }

    public class ReduceMaximumResourceMessage : EventMessage
    {
        public ResourceType Type;
        public int Amount;
        public bool Permanent;
    }

    public class DamageReportMessage : EventMessage
    {
        public int Amount;
        public DamageType Type;
    }

    public class SetDefaultInteractionRangeMessage : EventMessage
    {
        public int Range;
    }

    public class InteractWithObjectMessage : EventMessage
    {
        public InteractionType Type;
        public WorldObject Owner;
        public Action<string> OnError;
    }

    public class QueryShopItemByIdMessage : EventMessage
    {
        public string ShopId;
        public Action<ShopItemData> DoAfter;
    }

    public class QueryIsShopMessage : EventMessage
    {
        public Action DoAfter;
    }

    public class QueryInteractionMessage : EventMessage
    {
        public Action<InteractionType> DoAfter;
    }

    public class QueryInventoryItemByIdMessage : EventMessage
    {
        public string ItemId;
        public Action<ClientItemData> DoAfter;
    }

    public class RemoveInventoryItemByIdMessage : EventMessage
    {
        public string ItemId;
        public int Stack;
    }

    public class QueryPlayerClassDataMessage : EventMessage
    {
        public Action<string, int, int, int, ClientTalentData[]> DoAfter;
    }

    public class GainClassExperienceMessage : EventMessage
    {
        public int Amount;
    }

    public class SetClassExperienceSettingsMessage : EventMessage
    {
        public int BaseExperience;
        public float ExperienceMultiplier;
    }

    public class ApplyGrowthStatsMessage : EventMessage
    {
        public CombatGrowthStats Stats;
    }

    public class WipeCharactersMessage : EventMessage
    {
        public static WipeCharactersMessage INSTANCE = new WipeCharactersMessage();
    }

    public class ProcessClientInputMessage : EventMessage
    {
        public static ProcessClientInputMessage INSTANCE = new ProcessClientInputMessage();
    }

    public class QueryGlobalCooldownMessage : EventMessage
    {
        public Action<int> DoAfter;
    }

    public class TriggerGlobalCooldownMessage : EventMessage
    {
        public static TriggerGlobalCooldownMessage INSTANCE = new TriggerGlobalCooldownMessage();
    }

    public class AddAggrodMonsterMessage : EventMessage
    {
        public WorldObject Monster;
    }

    public class RemoveAggrodMonsterMessage : EventMessage
    {
        public WorldObject Monster;
    }

    public class BroadcastHealMessage : EventMessage
    {
        public int Amount;
        public WorldObject Owner;
    }

    public class FullHealMessage : EventMessage
    {
        public static FullHealMessage INSTANCE = new FullHealMessage();
    }

    public class StatusEffectCheckMessage : EventMessage
    {
        public StatusEffectType Type;
    }

    public class QueryStatusEffectsMessage : EventMessage
    {
        public Action<StatusEffectType> DoAfter;
    }

    public class ApplyStatusEffectMessage : EventMessage
    {
        public StatusEffectType Type;
        public WorldObject Owner;
    }

    public class QueryClientStatusEffectsMessage : EventMessage
    {
        public Action<ClientStatusEffectData> DoAfter;
    }

    public class FinishInteractionMessage : EventMessage
    {
        public WorldObject Owner;
    }

    public class InteractionFinishedMessage : EventMessage
    {
        public WorldObject Object;
    }

    public class SetMaxLevelMessage : EventMessage
    {
        public int Max;
    }

    public class LootItemMessage : EventMessage
    {
        public WorldObject Object;
        public string ItemId;
        public Action<string> OnError;
    }

    public class LootAllMessage : EventMessage
    {
        public WorldObject Object;
        public Action<string> OnError;
    }

    public class SetChestTicksMessage : EventMessage
    {
        public int Ticks;
    }

    public class SetCurrentCheckpointMessage : EventMessage
    {
        public Checkpoint Checkpoint;
    }

    public class SetDefaultCheckpointMessage : EventMessage
    {
        public string Default;
    }

    public class QueryCurrentCheckpointMessage : EventMessage
    {
        public Action<Checkpoint> DoAfter;
    }

    public class SetCullingBoxMessage : EventMessage
    {
        public Vector2IntData Box;
    }

    public class RefreshTimerMessage : EventMessage
    {
        public string Timer;
    }

    public class UpgradeAbilityMessage : EventMessage
    {
        public AbilityData Ability;
    }

    public class TransferToMapMessage : EventMessage
    {
        public string Map;
        public MapTile Tile;
    }

    public class QueryClientIconDataMessage : EventMessage
    {
        public Action<ClientObjectIconData> DoAfter;
    }

    public class AddAbilityModsMessage : EventMessage
    {
        public string Ability;
        public string[] Mods;
        public AbilityModType Type;
    }

    public class RemoveAbilityModsMessage : EventMessage
    {
        public string Ability;
        public string[] Mods;
        public AbilityModType Type;
    }

    public class AddWorldBonusMessage : EventMessage
    {
        public WorldBonusData Bonus;
    }

    public class RemoveWorldBonusMessage : EventMessage
    {
        public WorldBonusData Bonus;
    }

    public class QueryWorldBonusesMessage : EventMessage
    {
        public Action<WorldBonusData[]> DoAfter;
    }

    public class QueryWorldBonusesByTagsMessage : EventMessage
    {
        public string[] Tags;
        public WorldBonusType Type;
        public Action<WorldBonusData[]> DoAfter;
    }

    public class SetDefaultChatChannelsMessage : EventMessage
    {
        public string[] Channels;
    }
}