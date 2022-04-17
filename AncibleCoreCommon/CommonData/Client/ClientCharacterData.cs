using System;
using AncibleCoreCommon.CommonData.Combat;

namespace AncibleCoreCommon.CommonData.Client
{
    [Serializable]
    public class ClientCharacterData
    {
        public string Name;
        public string Sprite;
        public string Map;
        public string PlayerClass;
        public int Level;
        public int Experience;
        public int NextLevelExperience;
        public int UnspentTalentPoints;
        public ClientTalentData[] Talents;
        public Vector2IntData Position;
        public ClientAbilityInfoData[] Abilities;
        public int CurrentHealth;
        public ClientResourceData[] Resources;
        public CombatStats BaseStats;
        public CombatStats BonusStats;
        public ClientItemData[] Inventory;
        public ClientEquippedItemData[] Equipment;
        public ClientStatusEffectData[] StatusEffects;
        public IntNumberRange WeaponDamage;
        public ClientObjectIconData[] Icons;
        public string[] WorldBonuses;
        
        public int MaxInventorySlots;
        public int Gold;
    }
}