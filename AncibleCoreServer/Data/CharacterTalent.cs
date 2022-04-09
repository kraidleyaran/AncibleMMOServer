using System;
using AncibleCoreCommon.CommonData.Client;

namespace AncibleCoreServer.Data
{
    [Serializable]
    public class CharacterTalent : WorldData
    {
        public const string TABLE = "CharacterTalents";

        public string Name { get; set; }
        public int Rank { get; set; }

        public CharacterTalent()
        {

        }

        public CharacterTalent(ClientTalentData data)
        {
            FromData(data);
        }

        public void FromData(ClientTalentData data)
        {
            Name = data.Name;
            Rank = data.Rank;
        }
    }
}