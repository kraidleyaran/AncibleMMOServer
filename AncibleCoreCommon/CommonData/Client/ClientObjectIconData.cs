using System;

namespace AncibleCoreCommon.CommonData.Client
{
    [Serializable]
    public class ClientObjectIconData
    {
        public string Icon { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public int Ticks { get; set; }
        public int MaxTicks { get; set; }
        public ObjectIconType Type { get; set; }
    }
}