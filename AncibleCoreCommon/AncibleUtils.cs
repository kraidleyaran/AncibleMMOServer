using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Combat;
using Newtonsoft.Json;

namespace AncibleCoreCommon
{
    public static class AncibleUtils
    {
        public static T FromByteArray<T>(this byte[] data)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(data, 0, data.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                return (T)binForm.Deserialize(memStream);
            }
        }

        public static byte[] ToByeArry(this object obj)
        {
            var binaryFormatter = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                binaryFormatter.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static byte[] MessageToByteArray(this ClientMessage msg)
        {
            var binaryFormatter = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                binaryFormatter.Serialize(ms, msg);
                return ms.ToArray();
            }
            //var jsonString = JsonConvert.SerializeObject(msg, Formatting.Indented, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.None});
            //return Encoding.UTF8.GetBytes(jsonString);
        }

        public static ClientMessage ByteArrayToMessage(this byte[] bytes, SerializationBinder binder = null)
        {
            //var jsonString = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            //return JsonConvert.DeserializeObject<ClientMessage>(jsonString, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None});
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                if (binder != null)
                {
                    binForm.Binder = binder;
                }
                memStream.Write(bytes, 0, bytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                if (obj is ClientMessage message)
                {
                    return message;
                }
                return null;
            }
        }

        public static ClientMessage GenerateMessageFromJson(this byte[] data)
        {
            var json = Encoding.UTF8.GetString(data, 0, data.Length);
            var settings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.None};
            var original = JsonConvert.DeserializeObject<ClientMessage>(json, settings);
            switch (original.NetworkMessageId)
            {
                case ClientRegisterResultMessage.ID:
                    return JsonConvert.DeserializeObject<ClientRegisterResultMessage>(json, settings);
                case ClientLoginRequestMessage.ID:
                    return JsonConvert.DeserializeObject<ClientLoginRequestMessage>(json, settings);
                case ClientLoginResultMessage.ID:
                    return JsonConvert.DeserializeObject<ClientLoginResultMessage>(json, settings);
                case ClientCreateCharacterRequestMessage.ID:
                    return JsonConvert.DeserializeObject<ClientCreateCharacterRequestMessage>(json, settings);
                case ClientCreateCharacterResultMessage.ID:
                    return JsonConvert.DeserializeObject<ClientCreateCharacterResultMessage>(json, settings);
                case ClientCharacterRequestMessage.ID:
                    return JsonConvert.DeserializeObject<ClientCharacterRequestMessage>(json, settings);
                case ClientWorldTickMessage.ID:
                    return JsonConvert.DeserializeObject<ClientWorldTickMessage>(json, settings);
                case ClientEnterWorldWithCharacterRequestMessage.ID:
                    return JsonConvert.DeserializeObject<ClientEnterWorldWithCharacterRequestMessage>(json, settings);
                case ClientCheckInRequestMessage.ID:
                    return JsonConvert.DeserializeObject<ClientCheckInRequestMessage>(json, settings);
                case ClientCheckInResponseMessage.ID:
                    return JsonConvert.DeserializeObject<ClientCheckInResponseMessage>(json, settings);
                case ClientObjectUpdateMessage.ID:
                    return JsonConvert.DeserializeObject<ClientObjectUpdateMessage>(json, settings);
                case ClientMoveCommandMessage.ID:
                    return JsonConvert.DeserializeObject<ClientMoveCommandMessage>(json, settings);
                case ClientSetTickRateMessage.ID:
                    return JsonConvert.DeserializeObject<ClientSetTickRateMessage>(json, settings);
                case ClientMovementResponseMessage.ID:
                    return JsonConvert.DeserializeObject<ClientMovementResponseMessage>(json, settings);
                case ClientCharacterResultMessage.ID:
                    return JsonConvert.DeserializeObject<ClientCharacterResultMessage>(json, settings);
                case ClientEnterWorldWithCharacterResultMessage.ID:
                    return JsonConvert.DeserializeObject<ClientEnterWorldWithCharacterResultMessage>(json, settings);
                case ClientCharacterUpdateMessage.ID:
                    return JsonConvert.DeserializeObject<ClientCharacterUpdateMessage>(json, settings);
                case ClientUseAbilityRequestMessage.ID:
                    return JsonConvert.DeserializeObject<ClientUseAbilityRequestMessage>(json, settings);
                case ClientUseAbilityResultMessage.ID:
                    return JsonConvert.DeserializeObject<ClientUseAbilityResultMessage>(json, settings);
                case ClientWorldEventUpdateMessage.ID:
                    return JsonConvert.DeserializeObject<ClientWorldEventUpdateMessage>(json, settings);
                case ClientCastCancelledMessage.ID:
                    return JsonConvert.DeserializeObject<ClientCastCancelledMessage>(json, settings);
                case ClientPlayerDeadMessage.ID:
                    return JsonConvert.DeserializeObject<ClientPlayerDeadMessage>(json, settings);
                case ClientDeathConfirmationMessage.ID:
                    return JsonConvert.DeserializeObject<ClientDeathConfirmationMessage>(json, settings);
                case ClientPlayerRespawnMessage.ID:
                    return JsonConvert.DeserializeObject<ClientPlayerRespawnMessage>(json, settings);
                case ClientCastFailedMessage.ID:
                    return JsonConvert.DeserializeObject<ClientCastFailedMessage>(json, settings);
                case ClientUseItemMessage.ID:
                    return JsonConvert.DeserializeObject<ClientUseItemMessage>(json, settings);
                case ClientPlayerEventUpdateMessage.ID:
                    return JsonConvert.DeserializeObject<ClientPlayerEventUpdateMessage>(json, settings);
                case ClientEquipItemMessage.ID:
                    return JsonConvert.DeserializeObject<ClientEquipItemMessage>(json, settings);
                case ClientUnEquipItemFromSlotMessage.ID:
                    return JsonConvert.DeserializeObject<ClientUnEquipItemFromSlotMessage>(json, settings);
                case ClientInteractWithObjectRequestMessage.ID:
                    return JsonConvert.DeserializeObject<ClientInteractWithObjectRequestMessage>(json, settings);
                case ClientInteractWithObjectResultMessage.ID:
                    return JsonConvert.DeserializeObject<ClientInteractWithObjectResultMessage>(json, settings);
                case ClientBuyItemFromShopRequestMessage.ID:
                    return JsonConvert.DeserializeObject<ClientBuyItemFromShopRequestMessage>(json, settings);
                case ClientBuyItemFromShopResultMessage.ID:
                    return JsonConvert.DeserializeObject<ClientBuyItemFromShopResultMessage>(json, settings);
                case ClientSellItemToShopRequestMessage.ID:
                    return JsonConvert.DeserializeObject<ClientSellItemToShopRequestMessage>(json, settings);
                case ClientSellItemToShopResultMessage.ID:
                    return JsonConvert.DeserializeObject<ClientSellItemToShopResultMessage>(json, settings);
                case ClientFinishInteractionRequestMessage.ID:
                    return JsonConvert.DeserializeObject<ClientFinishInteractionRequestMessage>(json, settings);
                case ClientFinishInteractionResultMessage.ID:
                    return JsonConvert.DeserializeObject<ClientFinishInteractionResultMessage>(json, settings);
                case ClientShowShopMessage.ID:
                    return JsonConvert.DeserializeObject<ClientShowShopMessage>(json, settings);
                case ClientShowLootWindowMessage.ID:
                    return JsonConvert.DeserializeObject<ClientShowLootWindowMessage>(json, settings);
                case ClientLootItemRequestMessage.ID:
                    return JsonConvert.DeserializeObject<ClientLootItemRequestMessage>(json, settings);
                case ClientLootAllRequestMessage.ID:
                    return JsonConvert.DeserializeObject<ClientLootAllRequestMessage>(json, settings);
                case ClientLootItemResultMessage.ID:
                    return JsonConvert.DeserializeObject<ClientLootItemResultMessage>(json, settings);
                case ClientMoveItemToSlotRequestMessage.ID:
                    return JsonConvert.DeserializeObject<ClientMoveItemToSlotRequestMessage>(json, settings);
                case ClientMoveItemToSlotResultMessage.ID:
                    return JsonConvert.DeserializeObject<ClientMoveItemToSlotResultMessage>(json, settings);
                case ClientCombatSettingsUpdateMessage.ID:
                    return JsonConvert.DeserializeObject<ClientCombatSettingsUpdateMessage>(json, settings);
                case ClientTransferToMapMessage.ID:
                    return JsonConvert.DeserializeObject<ClientTransferToMapMessage>(json, settings);
                case ClientFinishMapTransferMessage.ID:
                    return JsonConvert.DeserializeObject<ClientFinishMapTransferMessage>(json, settings);
                case ClientTalentUpgradeRequestMessage.ID:
                    return JsonConvert.DeserializeObject<ClientTalentUpgradeRequestMessage>(json, settings);
                default:
                    return original;
            }

            
        }

        public static SecureLogin ConvertToSecureLogin(byte[] data)
        {
            var json = Encoding.UTF8.GetString(data, 0, data.Length);
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
            return JsonConvert.DeserializeObject<SecureLogin>(json, settings);
        }

        public static byte[] ToJson(this SecureLogin login)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
            var jsonString = JsonConvert.SerializeObject(login, settings);
            return Encoding.UTF8.GetBytes(jsonString);
        }

        public static byte[] ConvertToJson(this ClientMessage msg)
        {
            var jsonString = JsonConvert.SerializeObject(msg, Formatting.Indented, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.None});
            return Encoding.UTF8.GetBytes(jsonString);
        }



        public static List<string> ParseCommandLineToArguments(string cmdLine)
        {
            var args = new List<string>();
            if (string.IsNullOrWhiteSpace(cmdLine)) return args;

            var currentArg = new StringBuilder();
            bool inQuotedArg = false;

            for (int i = 0; i < cmdLine.Length; i++)
            {
                if (cmdLine[i] == '"')
                {
                    if (inQuotedArg)
                    {
                        args.Add(currentArg.ToString());
                        currentArg = new StringBuilder();
                        inQuotedArg = false;
                    }
                    else
                    {
                        inQuotedArg = true;
                    }
                }
                else if (cmdLine[i] == ' ')
                {
                    if (inQuotedArg)
                    {
                        currentArg.Append(cmdLine[i]);
                    }
                    else if (currentArg.Length > 0)
                    {
                        args.Add(currentArg.ToString());
                        currentArg = new StringBuilder();
                    }
                }
                else
                {
                    currentArg.Append(cmdLine[i]);
                }
            }

            if (currentArg.Length > 0) args.Add(currentArg.ToString());

            return args;
        }

        public static int CalculateExperienceForLevel(int baseExperience, float experienceFactor, int level)
        {
            var baseLevelExperience = baseExperience * level;
            var additionalExperience = baseLevelExperience * experienceFactor;
            return (int) (baseLevelExperience + additionalExperience);
        }

        public static string ToValueString(this int value)
        {
            if (value > 0)
            {
                return $"+{value}";
            }
            if (value < 0)
            {
                return $"{value}";
            }
            return string.Empty;
        }

        public static string ToValueString(this IntNumberRange range)
        {
            if (range.Minimum > 0)
            {
                return $"+ {range}";
            }
            else if (range.Maximum < 0)
            {
                return $"- {range}";
            }
            return string.Empty;
        }

        public static string ConverToJson<T>(T obj)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
            return JsonConvert.SerializeObject(obj, settings);
        }

        public static T FromJson<T>(string json)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        public static string ToPastTenseEffectString(this StatusEffectType type)
        {
            switch (type)
            {
                case StatusEffectType.Daze:
                    return "Dazed";
                case StatusEffectType.Pacify:
                    return "Pacified";
                case StatusEffectType.Root:
                    return "Rooted";
                case StatusEffectType.Sleep:
                    return "Slept";
                default:
                    return $"{type}";
            }
        }
    }


}