using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.Ability;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class UpgradeAbilityTrait : ObjectTrait
    {
        public override bool Instant => true;

        private string _ability = string.Empty;

        public UpgradeAbilityTrait(TraitData data) : base(data)
        {
            if (data is UpgradeAbilityTraitData upgradeData)
            {
                _ability = upgradeData.Ability;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            var ability = AbilityService.GetAbilityByName(_ability);
            this.SendMessageTo(new UpgradeAbilityMessage{Ability = ability}, _parent);
        }
    }
}