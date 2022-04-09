using System;

namespace AncibleCoreCommon.CommonData.Maps
{
    [Serializable]
    public class MapSpawnData
    {
        public string Map;
        public ObjectSpawnData[] Spawns;
    }
}