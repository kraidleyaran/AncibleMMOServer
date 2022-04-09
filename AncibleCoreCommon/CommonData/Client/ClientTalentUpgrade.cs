using System;

namespace AncibleCoreCommon.CommonData.Client
{
    [Serializable]
    public class ClientTalentUpgrade
    {
        public string Talent { get; set; }
        public int IncreasedRank { get; set; }
    }
}