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
        private string[] _mods = new string[0];
        private AbilityModType _type = AbilityModType.Owner;

        public ApplyAbilityModTrait(TraitData data) : base(data)
        {
            if (data is ApplyAbilityModTraitData abilityModData)
            {
                _ability = abilityModData.Ability;
                _mods = abilityModData.Mods;
                _type = abilityModData.ModType;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            this.SendMessageTo(new AddAbilityModsMessage{Ability = _ability, Mods = _mods, Type = _type}, _parent);
        }

        public override void ApplyRemoval(WorldObject owner)
        {
            base.ApplyRemoval(owner);
            this.SendMessageTo(new RemoveAbilityModsMessage{Ability = _ability, Mods = _mods, Type = _type}, _parent);
        }
    }
}