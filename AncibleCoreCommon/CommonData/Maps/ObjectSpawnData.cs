using System;

namespace AncibleCoreCommon.CommonData.Maps
{
    [Serializable]
    public class ObjectSpawnData
    {
        public string Name;
        public string Subtitle;
        public Vector2IntData Position;
        public string[] Traits;
        public bool Visible;
        public bool Blocking;
        public bool ShowName;
    }
}