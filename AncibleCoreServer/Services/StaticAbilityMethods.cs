using System.Collections.Generic;
using System.Linq;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Ability;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreCommon.CommonData.WorldEvent;
using AncibleCoreServer.Services.ObjectManager;
using AncibleCoreServer.Services.Traits;
using MessageBusLib;

namespace AncibleCoreServer.Services
{
    public static class StaticAbilityMethods
    {
        public static bool IsTargetOfType(this AbilityData ability, WorldObject owner, WorldObject target)
        {
            if (ability.TargetType == TargetType.Self && owner != target)
            {
                return false;
            }

            if (ability.TargetType == TargetType.Other && owner == target)
            {
                return false;
            }

            return true;
        }

        public static bool IsTargetOfAlignment(this AbilityData ability, WorldObject owner, WorldObject target)
        {
            if (ability.TargetAlignment == AbilityAlignment.All)
            {
                return true;
            }

            var ownerAlignment = CombatAlignment.Neutral;
            var targetAlignment = CombatAlignment.Neutral;

            var queryCombatAlignmentMsg = new QueryCombatAlignmentMessage
            {
                DoAfter = alignment => { ownerAlignment = alignment; }
            };
            owner.SendMessageTo(queryCombatAlignmentMsg, owner);
            queryCombatAlignmentMsg.DoAfter = alignment => { targetAlignment = alignment; };
            owner.SendMessageTo(queryCombatAlignmentMsg, target);

            if (ability.TargetAlignment == AbilityAlignment.Friendly)
            {
                return ownerAlignment == targetAlignment;
            }

            if (ability.TargetAlignment == AbilityAlignment.Enemy)
            {
                return ownerAlignment != targetAlignment;
            }

            return true;
        }

        public static ResourceType[] DoesOwnerHaveResources(this AbilityData ability, WorldObject owner)
        {
            var resources = new List<ResourceType>();
            if (ability.Resources.Length > 0)
            {
                var queryResourceByTypeMsg = new QueryResourceByTypeMessage();
                for (var i = 0; i < ability.Resources.Length; i++)
                {
                    var cost = ability.Resources[i];
                    queryResourceByTypeMsg.Type = cost.Type;
                    queryResourceByTypeMsg.DoAfter = resource =>
                    {
                        if (resource == null || cost.Amount > resource.Current)
                        {
                            resources.Add(cost.Type);
                        }
                    };
                    owner.SendMessageTo(queryResourceByTypeMsg, owner);
                }
            }

            return resources.ToArray();
        }

        public static bool IsTargetInRange(this AbilityData ability, WorldObject owner, WorldObject target)
        {
            return IsTargetInRange(ability, owner.Tile.Position, target.Tile.Position);
        }

        public static bool IsPositionInRange(this AbilityData ability, Vector2IntData position, WorldObject owner)
        {
            return owner.Tile.Position.Distance(position) <= ability.Range;
        }

        public static bool IsTargetInRange(this AbilityData ability, Vector2IntData origin, Vector2IntData destination)
        {
            if (ability.TargetType == TargetType.Self)
            {
                return true;
            }

            var distance = (int)origin.Distance(destination);
            return distance <= ability.Range;
        }

        public static void ApplyAbility(this AbilityData ability, WorldObject owner, WorldObject target, string[] targetMods, string[] ownerMods, int rank = 0)
        {
            if (ability.Resources.Length > 0)
            {
                var removeResourceMsg = new RemoveResourceMessage();
                for (var i = 0; i < ability.Resources.Length; i++)
                {
                    removeResourceMsg.Amount = ability.Resources[i].Amount;
                    removeResourceMsg.Type = ability.Resources[i].Type;
                    owner.SendMessageTo(removeResourceMsg, owner);
                }
            }

            var abilityRank = rank > ability.Upgrades.Length ? ability.Upgrades.Length - 1 : rank;
            owner.Tile.EventsOnTile.Add(new BumpWorldEvent { OriginId = owner.Id, TargetId = target.Id });
            var targetTraits = rank > 0 ? ability.Upgrades[abilityRank - 1].ApplyToTarget.Select(TraitService.GetTrait).Where(t => t != null).ToList() : ability.ApplyToTarget.Select(TraitService.GetTrait).Where(t => t != null).ToList();
            targetTraits.AddRange(targetMods.Select(TraitService.GetTrait).Where(m => m != null));

            var addTraitToObjMsg = new AddTraitToObjectMessage();
            for (var i = 0; i < targetTraits.Count; i++)
            {
                addTraitToObjMsg.Trait = targetTraits[i];
                owner.SendMessageTo(addTraitToObjMsg, target);
            }
            var ownerTraits = rank > 0 ? ability.Upgrades[abilityRank - 1].ApplyToOwner.Select(TraitService.GetTrait).Where(t => t != null).ToList() : ability.ApplyToOwner.Select(TraitService.GetTrait).Where(t => t != null).ToList();
            ownerTraits.AddRange(ownerMods.Select(TraitService.GetTrait).Where(m => m != null));
            for (var i = 0; i < ownerTraits.Count; i++)
            {
                addTraitToObjMsg.Trait = ownerTraits[i];
                owner.SendMessageTo(addTraitToObjMsg, owner);
            }
        }

    }
}