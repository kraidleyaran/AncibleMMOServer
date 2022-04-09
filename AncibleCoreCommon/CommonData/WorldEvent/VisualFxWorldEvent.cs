using System;
using Newtonsoft.Json;

namespace AncibleCoreCommon.CommonData.WorldEvent
{
    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class VisualFxWorldEvent : WorldEvent
    {
        public string OwnerId;
        public string VisualFx;
        public Vector2IntData OverridePosition;

        public VisualFxWorldEvent()
        {
            Type = WorldEventType.Fx;
        }
    }
}