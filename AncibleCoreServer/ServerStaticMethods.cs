using System;
using System.Collections.Generic;
using System.Linq;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Ability;
using AncibleCoreCommon.CommonData.CharacterClasses;
using AncibleCoreCommon.CommonData.Client;
using AncibleCoreCommon.CommonData.Combat;
using AncibleCoreCommon.CommonData.Items;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Data;
using AncibleCoreServer.Services;
using AncibleCoreServer.Services.Ability;
using AncibleCoreServer.Services.Items;
using AncibleCoreServer.Services.Traits;
using RogueSharp;

namespace AncibleCoreServer
{
    public static class ServerStaticMethods
    {

        public static ObjectTrait GetTraitFromData(this TraitData data)
        {
            switch (data.Type)
            {
                case SpriteTraitData.TYPE:
                    return new SpriteTrait(data);
                case AiStateTraitData.TYPE:
                    return new AiStateTrait(data);
                case AiWanderingTraitData.TYPE:
                    return new AiWanderingTrait(data);
                case AiMovementTraitData.TYPE:
                    return new AiMovementTrait(data);
                case ObjectStateTraitData.TYPE:
                    return new ObjectStateTrait();
                case CombatTraitData.TYPE:
                    return new CombatTrait(data);
                case AiAggroTraitData.TYPE:
                    return new AiAggroTrait(data);
                case DamageTraitData.TYPE:
                    return new DamageTrait(data);
                case MonsterCombatStatsTraitData.TYPE:
                    return new MonsterCombatStatsTrait(data);
                case AiAbilityManagerTraitData.TYPE:
                    return new AiAbilityManagerTrait(data);
                case CastingTraitData.TYPE:
                    return new CastingTrait(data);
                case ObjectSpawnerTraitData.TYPE:
                    return new ObjectSpawnerTrait(data);
                case MonsterLootTraitData.TYPE:
                    return new MonsterLootTrait(data);
                case HealTraitData.TYPE:
                    return new HealTrait(data);
                case ApplyCombatStatsTraitData.TYPE:
                    return new ApplyCombatStatsTrait(data);
                case WeaponDamageTraitData.TYPE:
                    return new WeaponDamageTrait(data);
                case ShopTraitData.TYPE:
                    return new ShopTrait(data);
                case ProjectileTraitData.TYPE:
                    return new ProjectileTrait(data);
                case VisualFxTraitData.TYPE:
                    return new VisualFxTrait(data);
                case StatusEffectTraitData.TYPE:
                    return new StatusEffectTrait(data);
                case OverTimeTraitData.TYPE:
                    return new OverTimeTrait(data);
                case TimerTraitData.TYPE:
                    return new TimerTrait(data);
                case AreaOfEffectTraitData.TYPE:
                    return new AreaOfEffectTrait(data);
                case ChanceToApplyTraitData.TYPE:
                    return new ChanceToApplyTrait(data);
                case HealerTraitData.TYPE:
                    return new HealerTrait(data);
                case AddAbilityTraitData.TYPE:
                    return new AddAbilityTrait(data);
                case AddResourceTraitData.TYPE:
                    return new AddResourceTrait(data);
                case RemoveResourceTraitData.TYPE:
                    return new RemoveResourceTrait(data);
                case ApplyResourceMaximumTraitData.TYPE:
                    return new ApplyResourceMaximumTrait(data);
                case DestroyObjectTraitData.TYPE:
                    return new DestroyObjectTrait();
                case CheckpointTraitData.TYPE:
                    return new CheckpointTrait(data);
                case RefreshTimerTraitData.TYPE:
                    return new RefreshTimerTrait(data);
                case UpgradeAbilityTraitData.TYPE:
                    return new UpgradeAbilityTrait(data);
                case MapTransferTraitData.TYPE:
                    return new MapTransferTrait(data);
                default:
                    return new ObjectTrait(data);
            }
        }

        public static ClientCharacterInfoData ToData(this WorldCharacter character)
        {
            return new ClientCharacterInfoData
            {
                Name = character.Name,
                Map = character.Map,
                Class = character.Class,
                Level = character.Level
            };
        }

        public static Vector2IntData Direction(this Vector2IntData origin, Vector2IntData destination)
        {
            var difference = destination - origin;
            var direction = Vector2IntData.zero;
            if (difference.X > 0)
            {
                direction.X = 1;
            }
            else if (difference.X < 0)
            {
                direction.X = -1;
            }

            if (difference.Y > 0)
            {
                direction.Y = 1;
            }
            else if (difference.Y < 0)
            {
                direction.Y = -1;
            }

            return direction;

        }

        public static double Distance(this Vector2IntData origin, Vector2IntData destination)
        {
            var vector = origin - destination;
            return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
        }

        public static CharacterAbility ToData(this AbilityData ability)
        {
            return new CharacterAbility {Name = ability.Name, Rank = 0};
        }

        public static CombatStats ToStats(this CharacterCombatStats stats)
        {
            return new CombatStats
            {
                Health = stats.Health,
                Strength = stats.Strength,
                Agility = stats.Agility,
                Intelligence = stats.Intelligence,
                Endurance = stats.Endurance,
                Dexterity = stats.Dexterity,
                Wisdom = stats.Wisdom,
                PhysicalDefense = stats.PhysicalDefense,
                MagicalDefense = stats.MagicalDefense
            };
        }

        public static CombatStats GetOverflow(this CombatGrowthStats growth)
        {
            var stats = new CombatStats
            {
                Health = (int) growth.Health,
                Strength = (int) growth.Strength,
                Agility = (int) growth.Agility,
                Intelligence = (int) growth.Intelligence,
                Endurance = (int) growth.Endurance,
                Wisdom = (int) growth.Wisdom,
                Dexterity = (int) growth.Dexterity,
                PhysicalDefense = (int) growth.PhysicalDefense,
                MagicalDefense = (int) growth.MagicalDefense
            };

            return stats;
        }

        public static CombatGrowthStats ToGrowthStats(this CharacterGrowthStats growth)
        {
            return new CombatGrowthStats
            {
                Health = growth.Health,
                Strength = growth.Strength,
                Agility = growth.Agility,
                Dexterity = growth.Dexterity,
                Endurance = growth.Endurance,
                Intelligence = growth.Intelligence,
                Wisdom = growth.Wisdom,
                PhysicalDefense = growth.PhysicalDefense,
                MagicalDefense = growth.MagicalDefense
            };
        }

        public static ItemStack[] GenerateItems(this LootTableData lootTable)
        {
            var items = new List<ItemStack>();
            var itemCount = lootTable.ItemDrops.GenerateRandomNumber(RNGService.RANDOM);
            if (itemCount > 0)
            {
                var roll = RNGService.GenerateRoll();
                var availableItems = lootTable.Items.Where(i => i.ChanceToDrop >= roll).ToArray();
                if (availableItems.Length > 0)
                {
                    if (availableItems.Length > 1)
                    {
                        for (var i = 0; i < itemCount; i++)
                        {
                            var item = availableItems[RNGService.RollRange(0, availableItems.Length)];
                            var existingItem = items.Find(it => it.Item.Name == item.Item);
                            if (existingItem == null)
                            {
                                var itemData = ItemService.GetItemByName(item.Item);
                                existingItem = new ItemStack { Item = itemData, Stack = 0 };
                                items.Add(existingItem);
                            }

                            existingItem.Stack += item.Stack.GenerateRandomNumber(RNGService.RANDOM);
                        }
                    }
                    else
                    {
                        var item = availableItems.Length > 1 ? availableItems[RNGService.RollRange(0, availableItems.Length)] : availableItems[0];
                        var existingItem = items.Find(it => it.Item.Name == item.Item);
                        if (existingItem == null)
                        {
                            var itemData = ItemService.GetItemByName(item.Item);
                            existingItem = new ItemStack { Item = itemData, Stack = 0 };
                            items.Add(existingItem);
                        }

                        existingItem.Stack += item.Stack.GenerateRandomNumber(RNGService.RANDOM);
                    }

                }
            }

            return items.ToArray();
        }

        public static ClientItemData ToData(this WorldItem item)
        {
            return new ClientItemData
            {
                Item = item.Item,
                Stack = item.Stack,
                ItemId = item.ItemId,
                Slot = item.Slot
            };
        }

        public static CharacterEquippedItem[] GenerateStartingEquipment(this CharacterClassData classData)
        {
            var equipment = new List<CharacterEquippedItem>();
            for (var i = 0; i < classData.StartingEquipment.Length; i++)
            {
                var item = ItemService.GetItemByName(classData.StartingEquipment[i]);
                if (item != null && item is EquippableItemData equippable)
                {
                    equipment.Add(new CharacterEquippedItem
                    {
                        Item = equippable.Name,
                        ItemId = Guid.NewGuid().ToString(),
                        Slot = equippable.Slot
                    });
                }
            }

            return equipment.ToArray();
        }

        public static AbilityData[] GenerateStartingAbilities(this CharacterClassData classData)
        {
            return classData.StartingAbilities.Select(AbilityService.GetAbilityByName).Where(a => a != null).ToArray();
        }

        public static CharacterResource[] GeneratePlayerStartingResources(this CharacterClassData classData)
        {
            return classData.Resources.Select(r => new CharacterResource(r)).ToArray();
        }

        public static WorldItem[] GeneratePlayerStartingItems(this CharacterClassData classData)
        {
            var items = new List<WorldItem>();
            for (var i = 0; i < classData.StartingItems.Length; i++)
            {
                items.Add(new WorldItem
                {
                    Item = classData.StartingItems[i].Item,
                    Stack = classData.StartingItems[i].Stack,
                    ItemId = Guid.NewGuid().ToString(),
                    Slot = i
                });
            }
            return items.ToArray();
        }

        public static IEnumerable<ICell> GetCellsInRectangle(this Map map, int top, int left, int width, int height)
        {
            var xMin = Math.Max(0, left);
            var xMax = Math.Min(map.Width, left + width);
            var yMin = Math.Max(0, top);
            var yMax = Math.Min(map.Height, top + height);

            for (var y = yMin; y < yMax; y++)
            {
                for (int x = xMin; x < xMax; x++)
                {
                    yield return map.GetCell(x, y);
                }
            }
        }

    }


}