using System;
using AncibleCoreCommon.CommonData.Client;

namespace AncibleCoreServer.Data
{
    [Serializable]
    public class CharacterAbility : WorldData
    {
        public const string TABLE = "CharacterAbility";
        public string Name { get; set; }
        public int Rank { get; set; }
        public int Cooldown { get; set; }
        public string[] OwnerMods { get; set; }
        public string [] TargetMods { get; set; }

        public void UpdateFromClientInfo(ClientAbilityInfoData data)
        {
            Rank = data.Rank;
            Cooldown = data.CurrentCooldownTicks;
            
        }

        
    }
}