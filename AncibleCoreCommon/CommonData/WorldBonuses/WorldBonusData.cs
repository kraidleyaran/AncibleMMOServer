using System;

namespace AncibleCoreCommon.CommonData.WorldBonuses
{
    [Serializable]
    public class WorldBonusData
    {
        public string Name;
        public WorldBonusType Type;
        public int Amount;
        public string[] Tags;
    }
}