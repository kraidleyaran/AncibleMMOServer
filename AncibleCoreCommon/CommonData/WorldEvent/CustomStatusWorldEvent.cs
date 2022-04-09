using System;

namespace AncibleCoreCommon.CommonData.WorldEvent
{
    [Serializable]
    public class CustomStatusWorldEvent : WorldEvent
    {
        public string Status { get; set; }
        public string OwnerId { get; set; }
        public string TargetId { get; set; }

        public CustomStatusWorldEvent()
        {
            Type = WorldEventType.CustomStatus;
        }
    }
}