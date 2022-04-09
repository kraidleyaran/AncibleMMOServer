using System;
using AncibleCoreCommon.CommonData.Ability;
using Newtonsoft.Json;

namespace AncibleCoreCommon.CommonData.WorldEvent
{
    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ResourceWorldEvent : WorldEvent
    {
        public string OwnerId { get; set; }
        public string TargetId { get; set; }
        public ResourceType Resource { get; set; }
        public int Amount { get; set; }

        public ResourceWorldEvent()
        {
            Type = WorldEventType.Resource;
        }
    }
}