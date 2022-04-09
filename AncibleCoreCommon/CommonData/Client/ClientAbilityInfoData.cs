using System;

namespace AncibleCoreCommon.CommonData.Client
{
    [Serializable]
    public class ClientAbilityInfoData
    {
        public string Name;
        public int Rank;
        public int Cooldown;
        public int CurrentCooldownTicks;
        public string[] TargetMods = new string[0];
        public string[] OwnerMods = new string[0];
    }
}