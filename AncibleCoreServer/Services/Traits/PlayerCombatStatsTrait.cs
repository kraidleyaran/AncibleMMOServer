using System.Collections.Generic;
using AncibleCoreCommon.CommonData.Combat;
using AncibleCoreCommon.CommonData.WorldEvent;
using AncibleCoreServer.Data;
using AncibleCoreServer.Services.Combat;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class PlayerCombatStatsTrait : ObjectTrait
    {
        public const string TRAIT_NAME = "Player Combat Stats Trait";

        private int _currentHealth = 0;

        private CombatStats _baseStats = new CombatStats();
        private CombatStats _bonusStats = new CombatStats();

        private List<WorldObject> _aggrodMonsters = new List<WorldObject>();

        private CombatGrowthStats _growthStats = new CombatGrowthStats();

        private TickTimer _healthRegenTimer = null;
        private TickTimer _healthCooldownTimer = null;

        public PlayerCombatStatsTrait(CharacterCombatStats stats, CharacterGrowthStats growth)
        {
            Name = TRAIT_NAME;
            _baseStats = stats.ToStats();
            _growthStats = growth.ToGrowthStats();
            _currentHealth = stats.CurrentHealth;

            if (_currentHealth > _baseStats.Health)
            {
                _currentHealth = _baseStats.Health;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            _healthRegenTimer = new TickTimer(CombatService.CombatSettings.HealthRegenTick, RegenHealth, -1);
            SubscribeToMessages();
        }

        private void RegenHealth()
        {
            if (_healthCooldownTimer == null)
            {
                var health = _currentHealth + CombatService.CalculateHealthRegen(_baseStats + _bonusStats);
                if (health > _baseStats.Health + _bonusStats.Health)
                {
                    health = _baseStats.Health + _bonusStats.Health;
                }

                _currentHealth = health;
                this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
            }
        }

        private void TriggerHealthRegenCooldown()
        {
            if (_healthCooldownTimer != null)
            {
                _healthCooldownTimer.Reset();
            }
            else
            {
                _healthCooldownTimer = new TickTimer(CombatService.CombatSettings.HealthRegenCooldown, () =>
                {
                    _healthCooldownTimer.Destroy();
                    _healthCooldownTimer = null;
                });
            }
        }

        private void SubscribeToMessages()
        {
            _parent.SubscribeWithFilter<QueryCombatStatsMessage>(QueryCombatStats, _instanceId);
            _parent.SubscribeWithFilter<QueryCombatGrowthStatsMessage>(QueryCombatGrowhStats, _instanceId);
            _parent.SubscribeWithFilter<TakeDamageMessage>(TakeDamage, _instanceId);
            _parent.SubscribeWithFilter<RespawnPlayerMessage>(RespawnPlayer, _instanceId);
            _parent.SubscribeWithFilter<QueryClientObjectDataMessage>(QueryClientObjectData, _instanceId);
            _parent.SubscribeWithFilter<ApplyCombatStatsMessage>(ApplyCombatStats, _instanceId);
            _parent.SubscribeWithFilter<RemoveCombatStatsMessage>(RemoveCombatStats, _instanceId);
            _parent.SubscribeWithFilter<HealMessage>(Heal, _instanceId);
            _parent.SubscribeWithFilter<ApplyGrowthStatsMessage>(ApplyGrowthStats, _instanceId);
            _parent.SubscribeWithFilter<AddAggrodMonsterMessage>(AddAggrodMonster, _instanceId);
            _parent.SubscribeWithFilter<RemoveAggrodMonsterMessage>(RemoveAggrodMonster, _instanceId);
            _parent.SubscribeWithFilter<FullHealMessage>(FullHeal, _instanceId);
            //_parent.SubscribeWithFilter<DamageReportMessage>(DamageReport, _instanceId);
        }

        private void QueryCombatStats(QueryCombatStatsMessage msg)
        {
            msg.DoAfter.Invoke(_baseStats, _bonusStats, _currentHealth);
        }

        private void QueryCombatGrowhStats(QueryCombatGrowthStatsMessage msg)
        {
            msg.DoAfter.Invoke(_growthStats);
        }

        private void TakeDamage(TakeDamageMessage msg)
        {
            _currentHealth -= msg.Amount;
            TriggerHealthRegenCooldown();
            this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                this.SendMessageTo(new SetObjectStateMessage{State = ObjectState.Dead}, _parent);
            }
        }

        private void RespawnPlayer(RespawnPlayerMessage msg)
        {
            _currentHealth = _baseStats.Health + _bonusStats.Health;
            this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
        }

        private void QueryClientObjectData(QueryClientObjectDataMessage msg)
        {
            msg.Data.Health = _currentHealth;
            msg.Data.MaxHealth = _baseStats.Health + _bonusStats.Health;
        }

        private void ApplyCombatStats(ApplyCombatStatsMessage msg)
        {
            if (msg.Permanent)
            {
                _baseStats += msg.Stats;
                _currentHealth += msg.Stats.Health;
                if (_currentHealth > _baseStats.Health + _bonusStats.Health)
                {
                    _currentHealth = _baseStats.Health + _bonusStats.Health;
                }
                else if (_currentHealth <= 0)
                {
                    _currentHealth = 1;
                }
            }
            else
            {
                _bonusStats += msg.Stats;
                _currentHealth += msg.Stats.Health;
                if (_currentHealth > _baseStats.Health + _bonusStats.Health)
                {
                    _currentHealth = _baseStats.Health + _bonusStats.Health;
                }
                else if (_currentHealth <= 0)
                {
                    _currentHealth = 1;
                }
            }

            this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
        }

        private void RemoveCombatStats(RemoveCombatStatsMessage msg)
        {
            if (msg.Permanent)
            {
                _baseStats -= msg.Stats;
                _currentHealth -= msg.Stats.Health;
                if (_currentHealth > _baseStats.Health + _bonusStats.Health)
                {
                    _currentHealth = _baseStats.Health + _bonusStats.Health;
                }
                else if (_currentHealth <= 0)
                {
                    _currentHealth = 1;
                }
            }
            else
            {
                _bonusStats -= msg.Stats;
                _currentHealth -= msg.Stats.Health;
                if (_currentHealth > _baseStats.Health + _bonusStats.Health)
                {
                    _currentHealth = _baseStats.Health + _bonusStats.Health;
                }
                else if (_currentHealth <= 0)
                {
                    _currentHealth = 1;
                }
            }

            this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
        }

        private void Heal(HealMessage msg)
        {
            var health = _currentHealth + msg.Amount;
            if (health > _baseStats.Health + _bonusStats.Health)
            {
                health = _baseStats.Health + _bonusStats.Health;
            }

            _currentHealth = health;
            this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
            if (msg.Broadcast && _aggrodMonsters.Count > 0)
            {
                var broadcastHealMsg = new BroadcastHealMessage();
                for (var i = 0; i < _aggrodMonsters.Count; i++)
                {
                    broadcastHealMsg.Amount = msg.Amount;
                    broadcastHealMsg.Owner = msg.Owner;
                    this.SendMessageTo(broadcastHealMsg, _aggrodMonsters[i]);
                }
            }
        }

        private void ApplyGrowthStats(ApplyGrowthStatsMessage msg)
        {
            _growthStats += msg.Stats;
            var overFlow = _growthStats.GetOverflow();
            _baseStats += overFlow;
            _growthStats -= overFlow;
            this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);

        }

        private void AddAggrodMonster(AddAggrodMonsterMessage msg)
        {
            if (!_aggrodMonsters.Contains(msg.Monster))
            {
                _aggrodMonsters.Add(msg.Monster);
            }
        }

        private void RemoveAggrodMonster(RemoveAggrodMonsterMessage msg)
        {
            _aggrodMonsters.Remove(msg.Monster);
        }

        private void FullHeal(FullHealMessage msg)
        {
            _currentHealth = _baseStats.Health + _bonusStats.Health;
            WorldObject parentObj = null;
            this.SendMessageTo(new QueryWorldObjectMessage{DoAfter = obj => parentObj = obj}, msg.Sender);
            _parent.Tile.EventsOnTile.Add(new CustomStatusWorldEvent{Status = "Healed", TargetId = _parent.Id, OwnerId = parentObj?.Id ?? string.Empty});
            this.SendMessageTo(FlagPlayerForUpdateMessage.INSTANCE, _parent);
        }

        private void DamageReport(DamageReportMessage msg)
        {
            TriggerHealthRegenCooldown();
        }

        public override void Destroy()
        {
            if (_healthRegenTimer != null)
            {
                _healthRegenTimer.Destroy();
                _healthRegenTimer = null;
            }

            if (_healthCooldownTimer != null)
            {
                _healthCooldownTimer.Destroy();
                _healthCooldownTimer = null;
            }
            base.Destroy();
        }
    }
}