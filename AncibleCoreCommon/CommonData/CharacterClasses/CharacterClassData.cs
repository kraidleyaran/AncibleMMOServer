using System;
using AncibleCoreCommon.CommonData.Client;
using AncibleCoreCommon.CommonData.Combat;

namespace AncibleCoreCommon.CommonData.CharacterClasses
{
    [Serializable]
    public class CharacterClassData
    {
        public string Class;
        public string[] Sprites;
        public CombatStats StartingStats;
        public CombatGrowthStats GrowthStats;
        public string[] StartingAbilities;
        public string[] StartingEquipment;
        public ClientItemData[] StartingItems;
        public string[] Talents;
        public ClassLevelUpData[] LevelUpData;
        public ClientResourceData[] Resources;
        
    }
}