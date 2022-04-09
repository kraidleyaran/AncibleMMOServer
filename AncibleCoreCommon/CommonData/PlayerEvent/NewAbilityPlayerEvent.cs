using System;
using Newtonsoft.Json;

namespace AncibleCoreCommon.CommonData.PlayerEvent
{
    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class NewAbilityPlayerEvent : PlayerEvent
    {
        public string Ability { get; set; }

        public NewAbilityPlayerEvent()
        {
            EventType = PlayerEventType.NewAbility;
        }
    }
}