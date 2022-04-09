using System;

namespace AncibleCoreCommon.CommonData.WorldEvent
{
    [Serializable]
    public class LevelUpWorldEvent : WorldEvent
    {
        public string OwnerId;
        public string OwnerName;
        public int Level;

        public LevelUpWorldEvent()
        {
            Type = WorldEventType.LevelUp;
        }
    }
}