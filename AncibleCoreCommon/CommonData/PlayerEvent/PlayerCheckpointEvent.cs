using System;
using Newtonsoft.Json;

namespace AncibleCoreCommon.CommonData.PlayerEvent
{
    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class PlayerCheckpointEvent : PlayerEvent
    {
        public string Checkpoint { get; set; }

        public PlayerCheckpointEvent()
        {
            EventType = PlayerEventType.Checkpoint;
        }

    }
}