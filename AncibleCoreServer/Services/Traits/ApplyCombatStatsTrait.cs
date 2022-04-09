using AncibleCoreCommon.CommonData.Combat;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class ApplyCombatStatsTrait : ObjectTrait
    {
        public override bool Instant => _permanent;
        private CombatStats _stats = new CombatStats();
        private bool _permanent = false;

        public ApplyCombatStatsTrait(TraitData data) : base(data)
        {
            if (data is ApplyCombatStatsTraitData statsData)
            {
                _stats = statsData.Stats;
                _permanent = statsData.Permanent;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            this.SendMessageTo(new ApplyCombatStatsMessage{Stats = _stats, Permanent = _permanent}, _parent);
        }

        public override void ApplyRemoval(WorldObject owner)
        {
            if (_permanent)
            {
                this.SendMessageTo(new RemoveCombatStatsMessage { Stats = _stats, Permanent = _permanent }, _parent);
            }
        }

        public override void Destroy()
        {
            if (!_permanent)
            {
                this.SendMessageTo(new RemoveCombatStatsMessage { Stats = _stats, Permanent = _permanent }, _parent);
            }
            base.Destroy();
        }
    }
}