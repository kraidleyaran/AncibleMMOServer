using System;

namespace AncibleCoreCommon.CommonData.Talents
{
    [Serializable]
    public class TalentData
    {
        public string Name;
        public int UnlockLevel;
        public TalentRankData[] Ranks;
        public string[] RequiredTalents;
    }
}