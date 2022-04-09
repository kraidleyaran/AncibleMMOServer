using System;

namespace AncibleCoreCommon.CommonData.Maps.TileEvents
{
    [Serializable]
    public class TransferTileEventData : TileEventData
    {
        public override TileEventType Type => TileEventType.Transfer;
        public string Map;
        public Vector2IntData Tile;
    }
}