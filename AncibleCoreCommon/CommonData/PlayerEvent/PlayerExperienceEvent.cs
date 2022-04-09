using System;
using Newtonsoft.Json;

namespace AncibleCoreCommon.CommonData.PlayerEvent
{
    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class PlayerExperienceEvent : PlayerEvent
    {
        public int Amount { get; set; }

        public PlayerExperienceEvent()
        {
            EventType = PlayerEventType.Experience;
        }
    }
}