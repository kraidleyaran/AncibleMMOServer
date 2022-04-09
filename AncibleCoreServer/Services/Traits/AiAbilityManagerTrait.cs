using System.Collections.Generic;
using System.Linq;
using AncibleCoreCommon.CommonData.Combat;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.Ability;
using AncibleCoreServer.Services.Maps;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class AiAbilityManagerTrait : ObjectTrait
    {
        private Dictionary<string, Ability.Ability> _abilities = new Dictionary<string, Ability.Ability>();
        private ObjectState _objectState = ObjectState.Active;

        public AiAbilityManagerTrait(TraitData data) : base(data)
        {
            if (data is AiAbilityManagerTraitData abilityManagerData)
            {
                for (var i = 0; i < abilityManagerData.Abilities.Length; i++)
                {
                    var ability = AbilityService.GetAbilityByName(abilityManagerData.Abilities[i]);
                    if (ability != null && !_abilities.ContainsKey(ability.Name))
                    {
                        _abilities.Add(ability.Name, new Ability.Ability(ability, new string[0], new string[0]));
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
            _parent.SubscribeWithFilter<AbilityCheckMessage>(AbilityCheck, _instanceId);
            _parent.SubscribeWithFilter<UpdateObjectStateMessage>(UpdateObjectState, _instanceId);
        }

        private void AbilityCheck(AbilityCheckMessage msg)
        {
            var statusEffects = new List<StatusEffectType>();
            this.SendMessageTo(new QueryStatusEffectsMessage{DoAfter = effect => statusEffects.Add(effect)}, _parent);
            if (!(statusEffects.Contains(StatusEffectType.Daze) || statusEffects.Contains(StatusEffectType.Pacify) || statusEffects.Contains(StatusEffectType.Sleep)))
            {
                
                var abilities = _abilities.Values.Where(a => !a.OnCooldown)
                    .Select(a => AbilityService.GetAbilityByName(a.Name))
                    .Where(a => a.IsTargetInRange(_parent, msg.Target) 
                                && a.IsTargetOfType(_parent, msg.Target) 
                                && a.IsTargetOfAlignment(_parent, msg.Target)).OrderByDescending(a => a.Range).ToArray();
                if (abilities.Length > 0)
                {
                    if (_objectState == ObjectState.Moving)
                    {
                        msg.OnAbilityUse(true);
                    }
                    else
                    {
                        var pov = MapService.GetMapTilesInArea(_parent.Map, _parent.Tile, abilities[0].Range + 5, true);
                        if (pov.FirstOrDefault(t => t == msg.Target.Tile) == null)
                        {
                            msg.OnAbilityUse(false);
                        }
                        else
                        {
                            var ability = abilities.Length > 1 ? abilities[RNGService.RollRange(0, abilities.Length)] : abilities[0];
                            var onAbilityUse = msg.OnAbilityUse;
                            this.SendMessageTo(new CastCommandMessage
                            {
                                Name = ability.Name,
                                DoAfter = () =>
                                {
                                    var targetState = ObjectState.Active;
                                    this.SendMessageTo(new QueryObjectStateMessage { DoAfter = objState => targetState = objState }, msg.Target);
                                    if (!msg.Target.BeingDestroy && targetState != ObjectState.Dead && ability.IsTargetInRange(_parent, msg.Target))
                                    {
                                        ability.ApplyAbility(_parent, msg.Target, new string[0], new string[0]);
                                        if (_abilities.TryGetValue(ability.Name, out var aiAbility))
                                        {
                                            aiAbility.TriggerCooldown();
                                        }
                                    }
                                },
                                Time = ability.CastTime,
                                OnSuccess = success => onAbilityUse(success),
                                Loop = 0
                            }, _parent);
                        }

                    }

                }
                else
                {
                    msg.OnAbilityUse(false);
                }
            }
        }

        private void UpdateObjectState(UpdateObjectStateMessage msg)
        {
            _objectState = msg.State;
        }
    }
}