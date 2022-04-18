using System;

namespace AncibleCoreCommon.CommonData.Client
{
    [Serializable]
    public class ClientCharacterInfoData
    {
        public string Name;
        public string Class;
        public string Map;
        public string Sprite;
        public int Level;
        public DateTime LastLogin = DateTime.MinValue;
    }
}