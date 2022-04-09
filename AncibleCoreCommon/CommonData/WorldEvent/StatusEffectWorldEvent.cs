using System;
using AncibleCoreCommon.CommonData.Combat;

namespace AncibleCoreCommon.CommonData.WorldEvent
{
    [Serializable]
    public class StatusEffectWorldEvent : WorldEvent
    {
        public StatusEffectType Effect { get; set; }
        public string OwnerId { get; set; }
        public string TargetId { get; set; }

        public StatusEffectWorldEvent()
        {
            Type = WorldEventType.StatusEffect;
        }
    }
}