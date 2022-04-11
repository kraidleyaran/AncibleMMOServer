using System;
using Newtonsoft.Json;

namespace AncibleCoreCommon.CommonData.Client
{
    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class ClientTalentUpgrade
    {
        public string Talent { get; set; }
        public int IncreasedRank { get; set; }
    }
}