using System;
using AncibleCoreCommon.CommonData.Traits;

namespace AncibleCoreCommon.CommonData.WorldEvent
{
    [Serializable]
    public class DamageEvent : WorldEvent
    {
        public DamageEvent()
        {
            Type = WorldEventType.Damage;
        }

        public string OriginId;
        public string TargetId;
        public int Amount;
        public DamageType DamageType;
        public bool CriticalStrike;
    }
}