using System;
using Newtonsoft.Json;

namespace AncibleCoreCommon.CommonData.WorldEvent
{
    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class CastWorldEvent : WorldEvent
    {
        public string OwnerId { get; set; }
        public int Length { get; set; }
        public string Ability { get; set; }

        public CastWorldEvent()
        {
            Type = WorldEventType.Cast;
        }
    }
}