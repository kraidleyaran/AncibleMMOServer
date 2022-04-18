using System;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Client;
using AncibleCoreCommon.CommonData.Combat;
using AncibleCoreCommon.CommonData.Items;
using AncibleCoreCommon.CommonData.WorldEvent;
using MessageBusLib;
using Newtonsoft.Json;

namespace AncibleCoreCommon
{   
    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientMessage : EventMessage
    {
        public string ClientId { get; set; }
        public string Filter { get; set; }
        public virtual int NetworkMessageId { get; set; }
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientRegisterResultMessage : ClientMessage
    {
        public const int ID = 1;
        public bool Success { get; set; }
        public string Message { get; set; }
        public byte[] Key { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientLoginRequestMessage : ClientMessage
    {
        public const int ID = 2;
        public byte[] Login { get; set; }
        public byte[] Key { get; set; }
        public byte[] Iv { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientLoginResultMessage : ClientMessage
    {
        public const int ID = 3;
        public bool Success { get; set; }
        public string Message { get; set; }
        public string[] StartingClasses { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientCreateCharacterRequestMessage : ClientMessage
    {
        public const int ID = 4;
        public string Name { get; set; }
        public string Class { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientCreateCharacterResultMessage : ClientMessage
    {
        public const int ID = 5;
        public bool Success { get; set; }
        public string Character { get; set; }
        public string Message { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientCharacterRequestMessage : ClientMessage
    {
        public const int ID = 6;
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientCharacterResultMessage : ClientMessage
    {
        public const int ID = 7;
        public ClientCharacterInfoData[] Characters { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientWorldTickMessage : ClientMessage
    {
        public const int ID = 8;
        public DateTime Server { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientEnterWorldWithCharacterRequestMessage : ClientMessage
    {
        public const int ID = 9;
        public string Name { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientEnterWorldWithCharacterResultMessage : ClientMessage
    {
        public const int ID = 10;
        public bool Success { get; set; }
        public ClientCharacterData Data { get; set; }
        public string ObjectId { get; set; }
        public string Message { get; set; }
        public int GlobalCooldown { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientCheckInRequestMessage : ClientMessage
    {
        public const int ID = 11;
        public static ClientCheckInRequestMessage INSTANCE = new ClientCheckInRequestMessage();
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientCheckInResponseMessage : ClientMessage
    {
        public const int ID = 12;
        public static ClientCheckInResponseMessage INSTANCE = new ClientCheckInResponseMessage();
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientObjectUpdateMessage : ClientMessage
    {
        public const int ID = 13;
        public ClientObjectData[] Objects { get; set; }
        public Vector2IntData[] Blocking { get; set; }
        public bool MapChange { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientMoveCommandMessage : ClientMessage
    {
        public const int ID = 14;
        public Vector2IntData Direction { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientSetTickRateMessage : ClientMessage
    {
        public const int ID = 15;
        public int TickRate { get; set; }
        public int GlobalCooldown { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientMovementResponseMessage : ClientMessage
    {
        public const int ID = 16;
        public bool Success { get; set; }
        public Vector2IntData Position { get; set; }
        public string Message { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientCharacterUpdateMessage : ClientMessage
    {
        public const int ID = 17;
        public ClientCharacterData Data { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientUseAbilityRequestMessage : ClientMessage
    {
        public const int ID = 18;
        public string Ability { get; set; }
        public string TargetId { get; set; }
        public Vector2IntData Position { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientUseAbilityResultMessage : ClientMessage
    {
        public const int ID = 19;
        public string Ability { get; set; }
        public int CastTime { get; set; }
        public string ObjectId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientWorldEventUpdateMessage : ClientMessage
    {
        public const int ID = 20;
        public string[] Events { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientCastCancelledMessage : ClientMessage
    {
        public const int ID = 21;
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientPlayerDeadMessage : ClientMessage
    {
        public const int ID = 22;
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientDeathConfirmationMessage : ClientMessage
    {
        public const int ID = 23;
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientPlayerRespawnMessage : ClientMessage
    {
        public const int ID = 24;
        public string Map { get; set; }
        public Vector2IntData Tile { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientCastFailedMessage : ClientMessage
    {
        public const int ID = 25;
        public string Reason { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientUseItemMessage : ClientMessage
    {
        public const int ID = 26;
        public string Name { get; set; }
        public string ItemId { get; set; }
        public override int NetworkMessageId => ID;
    }


    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientPlayerEventUpdateMessage : ClientMessage
    {
        public const int ID = 27;
        public string[] Events { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientEquipItemMessage : ClientMessage
    {
        public const int ID = 28;
        public string ItemId { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientUnEquipItemFromSlotMessage : ClientMessage
    {
        public const int ID = 29;
        public EquippableSlot Slot { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientInteractWithObjectRequestMessage : ClientMessage
    {
        public const int ID = 30;
        public InteractionType Interaction { get; set; }
        public string ObjectId { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientInteractWithObjectResultMessage : ClientMessage
    {
        public const int ID = 31;
        public bool Success { get; set; }
        public string Message { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientShowShopMessage : ClientMessage
    {
        public const int ID = 32;
        public ShopItemData[] ShopItems { get; set; }
        public string ObjectId { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientBuyItemFromShopRequestMessage : ClientMessage
    {
        public const int ID = 33;
        public string ObjectId { get; set; }
        public string ShopId { get; set; }
        public int Stack { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientSellItemToShopRequestMessage : ClientMessage
    {
        public const int ID = 34;
        public string ObjectId { get; set; }
        public string ItemId { get; set; }
        public int Stack { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientBuyItemFromShopResultMessage : ClientMessage
    {
        public const int ID = 35;
        public bool Success { get; set; }
        public int Cost { get; set; }
        public int Stack { get; set; }
        public string Item { get; set; }
        public string Message { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientSellItemToShopResultMessage : ClientMessage
    {
        public const int ID = 36;
        public bool Success { get; set; }
        public string Item { get; set; }
        public int Amount { get; set; }
        public int Stack { get; set; }
        public string Message { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientFinishInteractionRequestMessage : ClientMessage
    {
        public const int ID = 37;
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientFinishInteractionResultMessage : ClientMessage
    {
        public const int ID = 38;
        public bool Success { get; set; }
        public string Message { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientMoveItemToSlotRequestMessage : ClientMessage
    {
        public const int ID = 39;
        public string ItemId { get; set; }
        public int Slot { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientMoveItemToSlotResultMessage : ClientMessage
    {
        public const int ID = 40;
        public bool Success { get; set; }
        public string Message { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientLootItemRequestMessage : ClientMessage
    {
        public const int ID = 41;
        public string ItemId { get; set; }
        public string ObjectId { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientLootItemResultMessage : ClientMessage
    {
        public const int ID = 42;
        public bool Success { get; set; }
        public string Message { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientLootAllRequestMessage : ClientMessage
    {
        public const int ID = 43;
        public string ObjectId { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientShowLootWindowMessage : ClientMessage
    {
        public const int ID = 44;
        public string ObjectId { get; set; }
        public ClientLootItemData[] Loot { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientCombatSettingsUpdateMessage : ClientMessage
    {
        public const int ID = 45;
        public CombatSettings Settings { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientTransferToMapMessage : ClientMessage
    {
        public const int ID = 46;
        public string Map { get; set; }
        public Vector2IntData Tile { get; set; }
        public override int NetworkMessageId => ID;
    }


    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientFinishMapTransferMessage : ClientMessage
    {
        public const int ID = 47;
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientTalentUpgradeRequestMessage : ClientMessage
    {
        public const int ID = 48;
        public ClientTalentUpgrade[] Upgrades { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientChatUpdateMessage : ClientMessage
    {
        public const int ID = 49;
        public ChatMessageData[] Messages { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientChatMessage : ClientMessage
    {
        public const int ID = 50;
        public string Message { get; set; }
        public string Channel { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientJoinedChannelsMessage : ClientMessage
    {
        public const int ID = 51;
        public string[] Channels { get; set; }
        public override int NetworkMessageId => ID;
    }

    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientShowDialogueMessage : ClientMessage
    {
        public const int ID = 52;
        public string Dialogue { get; set; }
        public string OwnerId { get; set; }
        public override int NetworkMessageId => ID;
    }

    //TODO: Don't forget to add the message to the factory method in AncibleUtils
}