using System;
using Newtonsoft.Json;

namespace AncibleCoreCommon.CommonData.PlayerEvent
{
    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class PlayerUsedItemEvent : PlayerEvent
    {
        public string Item { get; set; }
        public PlayerUsedItemEvent()
        {
            EventType = PlayerEventType.UseItem;
        }
    }
}