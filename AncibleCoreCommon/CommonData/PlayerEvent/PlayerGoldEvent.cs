using System;
using Newtonsoft.Json;

namespace AncibleCoreCommon.CommonData.PlayerEvent
{
    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class PlayerGoldEvent : PlayerEvent
    {
        public int Amount { get; set; }

        public PlayerGoldEvent()
        {
            EventType = PlayerEventType.Gold;
        }
    }
}