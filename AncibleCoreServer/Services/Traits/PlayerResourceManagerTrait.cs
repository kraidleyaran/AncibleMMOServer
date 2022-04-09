using System;
using System.Collections.Generic;
using System.Linq;
using AncibleCoreCommon.CommonData.Ability;
using AncibleCoreCommon.CommonData.Client;
using AncibleCoreCommon.CommonData.Combat;
using AncibleCoreServer.Data;
using AncibleCoreServer.Services.Combat;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class PlayerResourceManagerTrait : ObjectTrait
    {
        private Dictionary<ResourceType, ClientResourceData> _resources = new Dictionary<ResourceType, ClientResourceData>();

        private TickTimer _manaRegenTimer = null;
        private TickTimer _focusRegenTimer = null;
        private TickTimer _spiritDegenTimer = null;

        public PlayerResourceManagerTrait(CharacterResource[] resources)
        {
            for (var i = 0; i < resources.Length; i++)
            {
                if (!_resources.ContainsKey(resources[i].Resource))
                {
                    _resources.Add(resources[i].Resource, new ClientResourceData{Resource = resources[i].Resource, Current = resources[i].Current, Maximum = resources[i].Maximum});
                }
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            if (_resources.ContainsKey(ResourceType.Mana))
            {
                _manaRegenTimer = new TickTimer(CombatService.ManaRegenTicks, RegenMana, -1);
            }

            if (_resources.ContainsKey(ResourceType.Focus))
            {
                _focusRegenTimer = new TickTimer(CombatService.FocusRegenTicks, RegenFocus, -1);
            }

            if (_resources.ContainsKey(ResourceType.Spirit))
            {
                _spiritDegenTimer = new TickTimer(CombatService.CombatSettings.SpiritDegenTick, DegenSpirit, -1);
                _resources[ResourceType.Spirit].Current = 0;
            }
            SubscribeToMessages();
        }

        private void RegenMana()
        {
            var combatStats = new CombatStats();
            this.SendMessageTo(new QueryCombatStatsMessage
            {
                DoAfter = (baseStats, bonusStats, health) =>
                {
                    combatStats = baseStats + bonusStats;
                }
            }, _parent);
            var mana = CombatService.CalculateRegenMana(combatStats);
            if (mana > 0)
            {
                this.SendMessageTo(new AddResourceMessage { Amount = mana, Type = ResourceType.Mana }, _parent);
            }
            
        }

        private void RegenFocus()
        {   
            this.SendMessageTo(new AddResourceMessage { Amount = CombatService.FocusPerRegen, Type = ResourceType.Focus }, _parent);
        }

        private void DegenSpirit()
        {
            this.SendMessageTo(new RemoveResourceMessage{Amount = 1, Type = ResourceType.Spirit}, _parent );
        }

        private void SubscribeToMessages()
        {
            _parent.SubscribeWithFilter<AddResourceMessage>(AddResource, _instanceId);
            _parent.SubscribeWithFilter<RemoveResourceMessage>(RemoveResource, _instanceId);
            _parent.SubscribeWithFilter<QueryResourceMessage>(QueryResources, _instanceId);
            _parent.SubscribeWithFilter<QueryResourceByTypeMessage>(QueryResourceByType, _instanceId);
            _parent.SubscribeWithFilter<ApplyMaximumResourceMessage>(ApplyMaximumResource, _instanceId);
            _parent.SubscribeWithFilter<ReduceMaximumResourceMessage>(ReduceMaximumResource, _instanceId);
            _parent.SubscribeWithFilter<RespawnPlayerMessage>(RespawnPlayer, _instanceId);
            _parent.SubscribeWithFilter<FullHealMessage>(FullHeal, _instanceId);

            if (_resources.ContainsKey(ResourceType.Spirit))
            {
                _parent.SubscribeWithFilter<TakeDamageMessage>(TakeDamage, _instanceId);
                _parent.SubscribeWithFilter<DamageReportMessage>(DamageReport, _instanceId);
            }
        }

        private void AddResource(AddResourceMessage msg)
        {
            if (_resources.TryGetValue(msg.Type, out var resource))
            {
                resource.Current += msg.Amount;
                resource.Current = Math.Max(0, Math.Min(resource.Current, resource.Maximum + resource.Bonus));
                this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
            }
        }

        private void RemoveResource(RemoveResourceMessage msg)
        {
            if (_resources.TryGetValue(msg.Type, out var resource))
            {
                resource.Current -= msg.Amount;
                resource.Current = Math.Max(0, Math.Min(resource.Current, resource.Maximum + resource.Bonus));
                this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
            }
        }

        private void QueryResources(QueryResourceMessage msg)
        {
            msg.DoAfter.Invoke(_resources.Values.ToArray());
        }

        private void QueryResourceByType(QueryResourceByTypeMessage msg)
        {
            msg.DoAfter.Invoke(_resources.TryGetValue(msg.Type, out var resource) ? resource : null);
        }

        private void ApplyMaximumResource(ApplyMaximumResourceMessage msg)
        {
            if (_resources.TryGetValue(msg.Type, out var resource))
            {
                
                if (msg.Permanent)
                {
                    resource.Maximum += msg.Amount;
                }
                else
                {
                    resource.Bonus += msg.Amount;
                }

                resource.Current += msg.Amount;
                if (resource.Current > resource.Maximum + resource.Bonus)
                {
                    resource.Current = resource.Maximum + resource.Bonus;
                }
                else if (resource.Current < 0)
                {
                    resource.Current = 0;
                }
                this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
                
            }
        }

        private void ReduceMaximumResource(ReduceMaximumResourceMessage msg)
        {
            if (_resources.TryGetValue(msg.Type, out var resource))
            {
                if (msg.Permanent)
                {
                    resource.Maximum -= msg.Amount;
                }
                else
                {
                    resource.Bonus -= msg.Amount;
                }

                resource.Current -= msg.Amount;
                if (resource.Current > resource.Maximum + resource.Bonus)
                {
                    resource.Current = resource.Maximum + resource.Bonus;
                }
                else if (resource.Current < 0)
                {
                    resource.Current = 0;
                }

                this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
            }
        }

        private void TakeDamage(TakeDamageMessage msg)
        {
            var combatStats = new CombatStats();
            this.SendMessageTo(new QueryCombatStatsMessage{DoAfter = (baseStats, bonusStats, health) =>
                {
                    combatStats = baseStats + bonusStats;
                }}, _parent);
            var amount = CombatService.CalculateIncomingSpirit(combatStats);
            if (amount > 0)
            {
                this.SendMessageTo(new AddResourceMessage{Amount = amount, Type = ResourceType.Spirit}, _parent);
            }
            _spiritDegenTimer.Reset();
        }

        private void DamageReport(DamageReportMessage msg)
        {
            //Disabled because doing damage and gaining spirit felt too powerful
            //var combatStats = new CombatStats();
            //this.SendMessageTo(new QueryCombatStatsMessage
            //{
            //    DoAfter = (baseStats, bonusStats, health) =>
            //    {
            //        combatStats = baseStats + bonusStats;
            //    }
            //}, _parent);
            //var amount = CombatService.CalculateOutgoingSpirit(combatStats);
            //if (amount > 0)
            //{
            //    this.SendMessageTo(new AddResourceMessage { Amount = amount, Type = ResourceType.Spirit }, _parent);
            //}
            _spiritDegenTimer.Reset();
        }

        private void RespawnPlayer(RespawnPlayerMessage msg)
        {
            var resources = _resources.Values.ToArray();
            for (var i = 0; i < resources.Length; i++)
            {
                switch (resources[i].Resource)
                {
                    case ResourceType.Spirit:
                        resources[i].Current = 0;
                        break;
                    case ResourceType.Mana:
                    case ResourceType.Focus:
                        resources[i].Current = resources[i].Maximum + resources[i].Bonus;
                        break;
                }
            }

            this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
        }

        private void FullHeal(FullHealMessage msg)
        {
            var resources = _resources.Values.ToArray();
            for (var i = 0; i < resources.Length; i++)
            {
                switch (resources[i].Resource)
                {
                    case ResourceType.Spirit:
                        //resources[i].Current = 0;
                        break;
                    case ResourceType.Mana:
                    case ResourceType.Focus:
                        resources[i].Current = resources[i].Maximum + resources[i].Bonus;
                        break;
                }
            }

            this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
        }

        public override void Destroy()
        {
            if (_focusRegenTimer != null)
            {
                _focusRegenTimer.Destroy();
                _focusRegenTimer = null;
            }

            if (_manaRegenTimer != null)
            {
                _manaRegenTimer.Destroy();
                _manaRegenTimer = null;
            }

            if (_spiritDegenTimer != null)
            {
                _spiritDegenTimer.Destroy();
                _spiritDegenTimer = null;
            }
            base.Destroy();
        }
    }
}