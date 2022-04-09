using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.Ability;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class AddAbilityTrait : ObjectTrait
    {
        public override bool Instant => true;

        private string _ability = string.Empty;

        public AddAbilityTrait(TraitData data) : base(data)
        {
            if (data is AddAbilityTraitData abilityData)
            {
                _ability = abilityData.Ability;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            var ability = AbilityService.GetAbilityByName(_ability);
            if (ability != null)
            {
                this.SendMessageTo(new AddAbilitiyMessage{Ability = ability}, _parent);
            }

        }
    }
}