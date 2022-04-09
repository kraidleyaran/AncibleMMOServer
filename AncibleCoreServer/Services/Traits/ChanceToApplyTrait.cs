using System.Linq;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class ChanceToApplyTrait : ObjectTrait
    {
        public override bool Instant => true;

        private float _chanceToApply;
        private string[] _applyOnChance;

        public ChanceToApplyTrait(TraitData data) : base(data)
        {
            if (data is ChanceToApplyTraitData chanceData)
            {
                _chanceToApply = chanceData.ChanceToApply;
                _applyOnChance = chanceData.ApplyOnChance;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            var apply = RNGService.Roll(_chanceToApply);
            if (apply)
            {
                var traits = _applyOnChance.Select(TraitService.GetTrait).Where(t => t != null).ToArray();
                if (traits.Length > 0)
                {
                    var addTraitsToObjMsg = new AddTraitToObjectMessage();
                    for (var i = 0; i < traits.Length; i++)
                    {
                        addTraitsToObjMsg.Trait = traits[i];
                        _sender.SendMessageTo(addTraitsToObjMsg, _parent);
                    }
                }
            }
        }
    }
}