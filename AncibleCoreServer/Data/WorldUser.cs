using System;

namespace AncibleCoreServer.Data
{
    [Serializable]
    public class WorldUser : WorldData
    {
        public static string TABLE => "WorldUsers";
        public string Username { get; set; }
        public string Password { get; set; }
        public string DataPath { get; set; }
        public string ResetKey { get; set; }
        public bool Active { get; set; }
        public int ActiveCharacterId { get; set; }
    }
}