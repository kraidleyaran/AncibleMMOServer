using System;

namespace AncibleCoreServer.Data
{
    [Serializable]
    public class GeneratedKeyData : WorldData
    {
        public const string TABLE = "GeneratedKeys";
        public string Name { get; set; }
        public string Key { get; set; }
        public string User { get; set; }
        public bool Claimed { get; set; }
    }
}