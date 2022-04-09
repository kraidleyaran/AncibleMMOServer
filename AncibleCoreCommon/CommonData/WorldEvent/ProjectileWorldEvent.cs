using System;
using Newtonsoft.Json;

namespace AncibleCoreCommon.CommonData.WorldEvent
{
    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ProjectileWorldEvent : WorldEvent
    {
        public string OwnerId;
        public string TargetId;
        public string Projectile;
        public int TravelTime;

        public ProjectileWorldEvent()
        {
            Type = WorldEventType.Projectile;
        }
    }
}