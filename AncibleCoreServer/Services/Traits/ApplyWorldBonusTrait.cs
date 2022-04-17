using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.ObjectManager;
using AncibleCoreServer.Services.WorldBonuses;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class ApplyWorldBonusTrait : ObjectTrait
    {
        public override bool Instant => _permanent;

        private string _bonus = string.Empty;
        private bool _permanent = false;

        public ApplyWorldBonusTrait(TraitData data) : base(data)
        {
            if (data is ApplyWorldBonusTraitData worldBonusData)
            {
                _bonus = worldBonusData.Bonus;
                _permanent = worldBonusData.Permanent;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            var bonus = WorldBonusService.GetBonusByName(_bonus);
            if (bonus != null)
            {
                this.SendMessageTo(new AddWorldBonusMessage { Bonus = bonus, Permanent = _permanent}, _parent);
            }
        }

        public override void ApplyRemoval(WorldObject owner)
        {
            if (_permanent)
            {
                var bonus = WorldBonusService.GetBonusByName(_bonus);
                if (bonus != null)
                {
                    this.SendMessageTo(new RemoveWorldBonusMessage { Bonus = bonus, Permanent = _permanent }, _parent);
                }
                base.ApplyRemoval(owner);
            }
        }

        public override void Destroy()
        {
            if (!_permanent)
            {
                var bonus = WorldBonusService.GetBonusByName(_bonus);
                if (bonus != null)
                {
                    this.SendMessageTo(new RemoveWorldBonusMessage { Bonus = bonus, Permanent = _permanent }, _parent);
                }
                
            }
            base.Destroy();
        }
    }
}