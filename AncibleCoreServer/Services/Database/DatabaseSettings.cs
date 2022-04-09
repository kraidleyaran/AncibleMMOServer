using System;

namespace AncibleCoreServer.Services.Database
{
    [Serializable]
    public class DatabaseSettings
    {
        public string Main;
        public string UserFolder;
        public string CharacterFolder;
        public string Analytics;
    }
}