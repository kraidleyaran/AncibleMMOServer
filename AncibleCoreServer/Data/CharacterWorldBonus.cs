using System;
using AncibleCoreCommon.CommonData.WorldBonuses;

namespace AncibleCoreServer.Data
{
    [Serializable]
    public class CharacterWorldBonus : WorldData
    {
        public const string TABLE = "CharacterWorldBonuses";

        public string Bonus { get; set; }

        public CharacterWorldBonus()
        {

        }

        public CharacterWorldBonus(WorldBonusData data)
        {
            FromWorldBonus(data);
        }

        public void FromWorldBonus(WorldBonusData data)
        {
            Bonus = data.Name;
        }
    }
}