using System;
using AncibleCoreCommon.CommonData.Combat;

namespace AncibleCoreCommon.CommonData.Client
{
    [Serializable]
    public class ClientStatusEffectData
    {
        public StatusEffectType Type { get; set; }
        public int CurrentTick { get; set; }
        public int MaxTick { get; set; }
    }
}