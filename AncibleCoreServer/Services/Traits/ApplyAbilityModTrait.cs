using AncibleCoreCommon.CommonData.Ability;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class ApplyAbilityModTrait : ObjectTrait
    {
        public override bool Instant => true;

        private string _ability = string.Empty;
        private string _mod = string.Empty;
        private AbilityModType _type = AbilityModType.Owner;

        public ApplyAbilityModTrait(TraitData data) : base(data)
        {
            if (data is ApplyAbilityModTraitData abilityModData)
            {
                _ability = abilityModData.Ability;
                _mod = abilityModData.Mod;
                _type = abilityModData.ModType;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            this.SendMessageTo(new AddAbilityModMessage{Ability = _ability, Mod = _mod, Type = _type}, _parent);
        }

        public override void ApplyRemoval(WorldObject owner)
        {
            base.ApplyRemoval(owner);
            this.SendMessageTo(new RemoveAbilityModMessage{Ability = _ability, Mod = _mod, Type = _type}, _parent);
        }
    }
}