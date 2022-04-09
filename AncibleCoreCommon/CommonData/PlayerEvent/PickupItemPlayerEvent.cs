using System;
using Newtonsoft.Json;

namespace AncibleCoreCommon.CommonData.PlayerEvent
{
    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class PickupItemPlayerEvent : PlayerEvent
    {
        public string ItemId;
        public int Stack;

        public PickupItemPlayerEvent()
        {
            EventType = PlayerEventType.PickupItem;
        }
    }
}