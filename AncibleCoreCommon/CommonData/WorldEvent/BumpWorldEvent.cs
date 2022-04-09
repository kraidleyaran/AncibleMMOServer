using System;
using Newtonsoft.Json;

namespace AncibleCoreCommon.CommonData.WorldEvent
{
    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class BumpWorldEvent : WorldEvent
    {
        public string OriginId;
        public string TargetId;

        public BumpWorldEvent()
        {
            Type = WorldEventType.Bump;
        }



        
    }
}