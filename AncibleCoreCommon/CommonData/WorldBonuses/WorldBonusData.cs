using System;
using AncibleCoreCommon.CommonData.Traits;

namespace AncibleCoreCommon.CommonData.WorldBonuses
{
    [Serializable]
    public class WorldBonusData
    {
        public string Name;
        public WorldBonusType Type;
        public float Amount;
        public ApplyValueType ApplyType;
        public string[] Tags;
    }
}