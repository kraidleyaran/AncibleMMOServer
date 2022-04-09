using System;
using Newtonsoft.Json;

namespace AncibleCoreCommon.CommonData.PlayerEvent
{
    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class AbilityRankUpPlayerEvent : PlayerEvent
    {
        public string Ability;
        public int Rank;

        public AbilityRankUpPlayerEvent()
        {
            EventType = PlayerEventType.AbilityRankUp;
        }
    }
}