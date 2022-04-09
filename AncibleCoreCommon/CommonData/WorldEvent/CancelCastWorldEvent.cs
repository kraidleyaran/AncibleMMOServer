using System;
using Newtonsoft.Json;

namespace AncibleCoreCommon.CommonData.WorldEvent
{
    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class CancelCastWorldEvent : WorldEvent
    {
        public string OwnerId { get; set; }

        public CancelCastWorldEvent()
        {
            Type = WorldEventType.CancelCast;
        }
    }
}