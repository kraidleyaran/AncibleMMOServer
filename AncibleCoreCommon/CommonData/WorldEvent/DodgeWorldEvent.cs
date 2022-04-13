using System;

namespace AncibleCoreCommon.CommonData.WorldEvent
{
    [Serializable]
    public class DodgeWorldEvent : WorldEvent
    {
        public string OriginId { get; set; }
        public string OwnerId { get; set; }

        public DodgeWorldEvent()
        {
            Type = WorldEventType.Dodge;
        }
    }
}