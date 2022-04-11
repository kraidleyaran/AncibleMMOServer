using System.Collections.Generic;
using System.Linq;
using AncibleCoreCommon;
using AncibleCoreCommon.CommonData.Ability;
using AncibleCoreCommon.CommonData.Combat;
using AncibleCoreCommon.CommonData.PlayerEvent;
using AncibleCoreServer.Data;
using AncibleCoreServer.Services.Ability;
using AncibleCoreServer.Services.Maps;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class PlayerAbilityManagerTrait : ObjectTrait
    {
        public const string NAME = "Player Ability Manager Trait";

        private Dictionary<string, Ability.Ability> _abilities = new Dictionary<string, Ability.Ability>();

        private string _playerId = string.Empty;
        private ObjectState _objectState = ObjectState.Active;

        public PlayerAbilityManagerTrait(string playerId, CharacterAbility[] abilities)
        {
            Name = NAME;
            _playerId = playerId;
            for (var i = 0; i < abilities.Length; i++)
            {
                if (!_abilities.ContainsKey(abilities[i].Name))
                {
                    var abilityData = AbilityService.GetAbilityByName(abilities[i].Name);
                    if (abilityData != null)
                    {
                        _abilities.Add(abilities[i].Name, new Ability.Ability(abilityData, abilities[i].OwnerMods, abilities[i].TargetMods, abilities[i].Rank, abilities[i].Cooldown));
                    }
                    
                }
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            this.SubscribeWithFilter<ClientUseAbilityRequestMessage>(ClientUseAbilityRequest, _playerId);

            _parent.SubscribeWithFilter<QueryAbilitiesMessage>(QueryAbilities, _instanceId);
            _parent.SubscribeWithFilter<AddAbilitiyMessage>(AddAbility, _instanceId);
            _parent.SubscribeWithFilter<RemoveAbilityMessage>(RemoveAbility, _instanceId);
            _parent.SubscribeWithFilter<UpdateObjectStateMessage>(UpdateObjectState, _instanceId);
            _parent.SubscribeWithFilter<UpgradeAbilityMessage>(UpgradeAbility, _instanceId);
            _parent.SubscribeWithFilter<AddAbilityModsMessage>(AddAbilityMod, _instanceId);
            _parent.SubscribeWithFilter<RemoveAbilityModsMessage>(RemoveAbilityMod, _instanceId);
        }

        private void AddAbility(AddAbilitiyMessage msg)
        {
            if (!_abilities.ContainsKey(msg.Ability.Name))
            {
                _abilities.Add(msg.Ability.Name, new Ability.Ability(msg.Ability, new string[0], new string[0]));
                this.SendMessageTo(new RegisterPlayerEventMessage { Event = new NewAbilityPlayerEvent { Ability = msg.Ability.Name } }, _parent);
                this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
            }
        }

        private void RemoveAbility(RemoveAbilityMessage msg)
        {
            _abilities.Remove(msg.Ability);
            this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
        }

        private void QueryAbilities(QueryAbilitiesMessage msg)
        {
            msg.DoAfter.Invoke(_abilities.Values.ToArray());
        }

        private void ClientUseAbilityRequest(ClientUseAbilityRequestMessage msg)
        {
            var statusEffects = new List<StatusEffectType>();
            this.SendMessageTo(new QueryStatusEffectsMessage{DoAfter = effect => statusEffects.Add(effect)}, _parent);
            if (statusEffects.Contains(StatusEffectType.Daze) || statusEffects.Contains(StatusEffectType.Pacify))
            {
                var effects = statusEffects.Where(e => e == StatusEffectType.Daze || e == StatusEffectType.Pacify).ToArray();
                var effectsString = string.Empty;
                for (var i = 0; i < effects.Length; i++)
                {
                    effectsString = i < effects.Length - 1 ? $"{effectsString}{effects[i].ToPastTenseEffectString()}," : $"{effectsString}{effects[i].ToPastTenseEffectString()}";
                }
                this.SendMessageTo(new ClientUseAbilityResultMessage { Message = $"Cannot use an ability while {effectsString}.", Ability = msg.Ability, Success = false }, _parent);
            }
            else if (_objectState == ObjectState.Dead)
            {
                this.SendMessageTo(new ClientUseAbilityResultMessage { Message = $"Cannot use an ability while dead.", Ability = msg.Ability, Success = false }, _parent);
            }
            else if (_objectState == ObjectState.Moving)
            {
                this.SendMessageTo(new ClientUseAbilityResultMessage { Message = $"Cannot use an ability while moving.", Ability = msg.Ability, Success = false }, _parent);
            }
            else
            {
                
                if (_abilities.TryGetValue(msg.Ability, out var ability))
                {
                    
                    var abilityData = AbilityService.GetAbilityByName(ability.Name);
                    if (abilityData != null)
                    {
                        
                        var targetObj = ObjectManagerService.GetObjectFromId(msg.TargetId);

                        if (abilityData.TargetType != TargetType.Position && targetObj == null)
                        {
                            this.SendMessageTo(new ClientUseAbilityResultMessage { Message = $"Requires target", Ability = msg.Ability, Success = false }, _parent);
                            return;
                        }

                        if (abilityData.TargetType != TargetType.Position && targetObj.Map != _parent.Map)
                        {
                            this.SendMessageTo(new ClientUseAbilityResultMessage { Message = $"Requires target", Ability = msg.Ability, Success = false }, _parent);
                            return;
                        }


                        var success = false;
                        if (ability.OnCooldown)
                        {
                            this.SendMessageTo(new ClientUseAbilityResultMessage { Message = "On cooldown", Ability = msg.Ability, Success = false }, _parent);
                            return;
                        }
                        if (abilityData.TargetType == TargetType.Position)
                        {
                            if (!abilityData.IsPositionInRange(msg.Position, _parent))
                            {
                                this.SendMessageTo(new ClientUseAbilityResultMessage { Message = $"Out of range", Ability = msg.Ability, Success = false }, _parent);
                                return;
                            }
                        }
                        else if (!abilityData.IsTargetInRange(_parent, targetObj))
                        {
                            this.SendMessageTo(new ClientUseAbilityResultMessage { Message = $"Out of range", Ability = msg.Ability, Success = false }, _parent);
                            return;
                        }
                        var pov = MapService.GetMapTilesInArea(_parent.Map, _parent.Tile, abilityData.Range + 5, true);

                        if (abilityData.TargetType != TargetType.Self)
                        {
                            if (abilityData.TargetType == TargetType.Position)
                            {
                                var targetTile = MapService.GetMapTileInMapByPosition(_parent.Map, msg.Position);
                                if (targetTile == null || pov.FirstOrDefault(t => t == targetTile) == null)
                                {
                                    this.SendMessageTo(new ClientUseAbilityResultMessage { Message = "No Line of Sight", Ability = msg.Ability, Success = false }, _parent);
                                    return;
                                }
                            }
                            else
                            {
                                if (pov.FirstOrDefault(t => t == targetObj.Tile) == null)
                                {
                                    this.SendMessageTo(new ClientUseAbilityResultMessage { Message = "No Line of Sight", Ability = msg.Ability, Success = false }, _parent);
                                    return;
                                }
                            }

                        }
                        if (abilityData.TargetType != TargetType.Position && !abilityData.IsTargetOfType(_parent, targetObj) || !abilityData.IsTargetOfAlignment(_parent, targetObj))
                        {
                            this.SendMessageTo(new ClientUseAbilityResultMessage { Message = $"Invalid target", Ability = msg.Ability, Success = false }, _parent);
                            return;
                        }

                        var globalCooldown = 0;
                        this.SendMessageTo(new QueryGlobalCooldownMessage { DoAfter = cooldown => globalCooldown = cooldown }, _parent);
                        if (globalCooldown > 0)
                        {
                            this.SendMessageTo(new ClientUseAbilityResultMessage { Message = $"On Cooldown", Ability = msg.Ability, Success = false }, _parent);
                            return;
                        }

                        var missingResources = abilityData.DoesOwnerHaveResources(_parent);
                        if (missingResources.Length > 0)
                        {
                            var message = $"Not enough";
                            for (var i = 0; i < missingResources.Length; i++)
                            {
                                message = i > missingResources.Length - 1 ? $"{message} {missingResources[i]}," : $"{message} {missingResources[i]}";
                            }
                            this.SendMessageTo(new ClientUseAbilityResultMessage { Message = $"{message}", Ability = msg.Ability, Success = false }, _parent);
                        }
                        else
                        {
                            this.SendMessageTo(new CastCommandMessage
                            {
                                DoAfter = () =>
                                {
                                    if (abilityData.TargetType != TargetType.Position && targetObj.BeingDestroy)
                                    {
                                        this.SendMessageTo(new ClientCastFailedMessage { Reason = "Target is already dead" }, _parent);
                                    }
                                    else
                                    {
                                        if (abilityData.TargetType == TargetType.Position)
                                        {
                                            targetObj = ObjectManagerService.GenerateObject(_parent.Map, msg.Position, abilityData.PositionName);
                                        }
                                        abilityData.ApplyAbility(_parent, targetObj, ability.TargetMods.ToArray(), ability.OwnerMods.ToArray());
                                        ability.TriggerCooldown();
                                        this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
                                    }

                                },
                                Loop = 0,
                                Name = ability.Name,
                                Time = abilityData.CastTime,
                                OnSuccess = isSuccess => { success = isSuccess; }
                            }, _parent);
                            this.SendMessageTo(success ? new ClientUseAbilityResultMessage { Success = true, Ability = abilityData.Name, CastTime = abilityData.CastTime, ObjectId = targetObj.Id }
                                    : new ClientUseAbilityResultMessage
                                    {
                                        Message = $"Busy",
                                        Ability = msg.Ability,
                                        Success = false
                                    }, _parent);
                        }
                    }
                    else
                    {
                        this.SendMessageTo(new ClientUseAbilityResultMessage { Message = $"Invalid Ability", Ability = msg.Ability, Success = false }, _parent);
                    }

                }
                else
                {
                    this.SendMessageTo(new ClientUseAbilityResultMessage { Message = $"You do not have that ability", Ability = msg.Ability, Success = false }, _parent);
                }
            }

        }

        private void UpdateObjectState(UpdateObjectStateMessage msg)
        {
            _objectState = msg.State;
        }

        private void UpgradeAbility(UpgradeAbilityMessage msg)
        {
            if (_abilities.TryGetValue(msg.Ability.Name, out var ability))
            {
                if (msg.Ability.Upgrades.Length > 0 && ability.Rank - 1 < msg.Ability.Upgrades.Length)
                {
                    ability.Rank++;
                    this.SendMessageTo(new RegisterPlayerEventMessage{Event = new AbilityRankUpPlayerEvent{Ability = ability.Name, Rank = ability.Rank}}, _parent);
                }
            }
        }

        private void AddAbilityMod(AddAbilityModsMessage msg)
        {
            if (_abilities.TryGetValue(msg.Ability, out var ability))
            {
                switch (msg.Type)
                {
                    case AbilityModType.Target:
                        ability.TargetMods.AddRange(msg.Mods);
                        break;
                    case AbilityModType.Owner:
                        ability.OwnerMods.AddRange(msg.Mods);
                        break;
                }
            }
        }

        private void RemoveAbilityMod(RemoveAbilityModsMessage msg)
        {
            if (_abilities.TryGetValue(msg.Ability, out var ability))
            {
                switch (msg.Type)
                {
                    case AbilityModType.Target:
                        ability.TargetMods.AddRange(msg.Mods);
                        break;
                    case AbilityModType.Owner:
                        ability.OwnerMods.AddRange(msg.Mods);
                        break;
                }
            }
        }

    }
}