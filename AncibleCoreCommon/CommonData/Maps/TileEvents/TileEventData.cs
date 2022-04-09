using System;

namespace AncibleCoreCommon.CommonData.Maps.TileEvents
{
    [Serializable]
    public class TileEventData
    {
        public virtual TileEventType Type => TileEventType.Default;
    }
}