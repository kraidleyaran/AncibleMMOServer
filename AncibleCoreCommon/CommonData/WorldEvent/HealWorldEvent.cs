using System;
using Newtonsoft.Json;

namespace AncibleCoreCommon.CommonData.WorldEvent
{
    [Serializable]
    [JsonObject(MemberSerialization.OptOut)]
    public class HealWorldEvent : WorldEvent
    {
        public string OwnerId { get; set; }
        public string TargetId { get; set; }
        public int Amount { get; set; }

        public HealWorldEvent()
        {
            Type = WorldEventType.Heal;
        }
    }
}