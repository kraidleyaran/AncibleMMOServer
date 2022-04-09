using AncibleCoreCommon.CommonData.Combat;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class MonsterCombatStatsTrait : ObjectTrait
    {
        private MonsterCombatStats _combatStats = new MonsterCombatStats();
        private MonsterCombatStats _bonusCombatStats = new MonsterCombatStats();

        private int _currentHealth = 0;

        private ObjectState _objState = ObjectState.Active;

        public MonsterCombatStatsTrait(TraitData data) : base(data)
        {
            if (data is MonsterCombatStatsTraitData monsterStatsData)
            {
                _combatStats = monsterStatsData.Stats;
                _currentHealth = _combatStats.Health;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _parent.SubscribeWithFilter<TakeDamageMessage>(TakeDamage, _instanceId);
            _parent.SubscribeWithFilter<QueryClientObjectDataMessage>(QueryClientObjectData, _instanceId);
            _parent.SubscribeWithFilter<UpdateObjectStateMessage>(UpdateObjectState, _instanceId);
            _parent.SubscribeWithFilter<ApplyCombatStatsMessage>(ApplyCombatStats, _instanceId);
            _parent.SubscribeWithFilter<RemoveCombatStatsMessage>(RemoveCombatStats, _instanceId);
            _parent.SubscribeWithFilter<QueryCombatStatsMessage>(QueryCombatStats, _instanceId);
            _parent.SubscribeWithFilter<FullHealMessage>(FullHeal, _instanceId);
        }
        
        private void TakeDamage(TakeDamageMessage msg)
        {
            if (_objState != ObjectState.Dead)
            {
                _currentHealth -= msg.Amount;
                WorldObject damageOwner = null;
                this.SendMessageTo(new QueryWorldObjectMessage{DoAfter = obj => damageOwner = obj}, msg.Sender);
                if (damageOwner != null)
                {
                    this.SendMessageTo(new AddLooterMessage{Obj = damageOwner}, _parent);
                }
                if (_currentHealth <= 0)
                {
                    this.SendMessageTo(SpawnLootMessage.INSTANCE, _parent);
                    this.SendMessageTo(new SetObjectStateMessage { State = ObjectState.Dead }, _parent);
                }
            }
        }

        private void QueryClientObjectData(QueryClientObjectDataMessage msg)
        {
            var health = (float)_currentHealth / (_combatStats.Health + _bonusCombatStats.Health) * 100f;
            msg.Data.Health = (int)health;
            msg.Data.MaxHealth = 100;
        }

        private void UpdateObjectState(UpdateObjectStateMessage msg)
        {
            _objState = msg.State;

        }

        private void ApplyCombatStats(ApplyCombatStatsMessage msg)
        {
            _bonusCombatStats += msg.Stats;
            if (_currentHealth > _combatStats.Health + _bonusCombatStats.Health)
            {
                _currentHealth = _bonusCombatStats.Health + _bonusCombatStats.Health;
            }
        }

        private void RemoveCombatStats(RemoveCombatStatsMessage msg)
        {
            _bonusCombatStats -= msg.Stats;
            if (_currentHealth > _combatStats.Health + _bonusCombatStats.Health)
            {
                _currentHealth = _bonusCombatStats.Health + _bonusCombatStats.Health;
            }
        }

        private void QueryCombatStats(QueryCombatStatsMessage msg)
        {
            var baseStats = new CombatStats(_combatStats);
            var bonusStats = new CombatStats(_bonusCombatStats);
            msg.DoAfter.Invoke(baseStats, bonusStats, _currentHealth);
        }

        private void FullHeal(FullHealMessage msg)
        {
            _currentHealth = _combatStats.Health + _bonusCombatStats.Health;
        }
    }
}