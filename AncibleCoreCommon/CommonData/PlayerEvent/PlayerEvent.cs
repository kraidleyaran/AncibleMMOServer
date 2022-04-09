using System;
using Newtonsoft.Json;

namespace AncibleCoreCommon.CommonData.PlayerEvent
{
    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class PlayerEvent
    {
        public PlayerEventType EventType { get; set; }
        public string EventMessage { get; set; }
    }
}