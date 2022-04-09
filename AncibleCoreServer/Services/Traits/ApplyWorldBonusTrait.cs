using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.ObjectManager;
using AncibleCoreServer.Services.WorldBonuses;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class ApplyWorldBonusTrait : ObjectTrait
    {
        public override bool Instant => true;

        private string _bonus = string.Empty;

        public ApplyWorldBonusTrait(TraitData data) : base(data)
        {
            if (data is ApplyWorldBonusTraitData worldBonusData)
            {
                _bonus = worldBonusData.Bonus;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            var bonus = WorldBonusService.GetBonusByName(_bonus);
            if (bonus != null)
            {
                this.SendMessageTo(new AddWorldBonusMessage { Bonus = bonus}, _parent);
            }
        }
    }
}