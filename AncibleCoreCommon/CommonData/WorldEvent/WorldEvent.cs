using System;
using Newtonsoft.Json;

namespace AncibleCoreCommon.CommonData.WorldEvent
{
    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class WorldEvent
    {
        public string Name;
        public string Text;
        public WorldEventType Type;


        public virtual string ToJson()
        {
            return AncibleUtils.ConverToJson(this);
        }
    }
}