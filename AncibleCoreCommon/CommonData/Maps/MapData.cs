using System;

namespace AncibleCoreCommon.CommonData.Maps
{
    [Serializable]
    public class MapData
    {
        public string Name;
        public MapTileData[] Tiles;
        public Vector2IntData Size;
        public Vector2IntData Offset;
    }
}