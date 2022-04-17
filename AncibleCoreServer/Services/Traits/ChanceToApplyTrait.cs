using System.Linq;
using AncibleCoreCommon;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreCommon.CommonData.WorldBonuses;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class ChanceToApplyTrait : ObjectTrait
    {
        public override bool Instant => true;

        private float _chanceToApply;
        private string[] _applyOnChance = new string[0];
        private string[] _tags = new string[0];

        public ChanceToApplyTrait(TraitData data) : base(data)
        {
            if (data is ChanceToApplyTraitData chanceData)
            {
                _chanceToApply = chanceData.ChanceToApply;
                _applyOnChance = chanceData.ApplyOnChance;
                _tags = chanceData.Tags;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            var chance = _chanceToApply;
            if (_tags.Length > 0)
            {
                var sender = _parent;
                this.SendMessageTo(new QueryWorldObjectMessage{DoAfter = parentOwner => sender = parentOwner}, _sender);
                var worldBonuses = new WorldBonusData[0];
                this.SendMessageTo(new QueryWorldBonusesByTagsMessage { Type = WorldBonusType.Chance, Tags = _tags, DoAfter = bonuses => worldBonuses = bonuses }, sender);
                if (worldBonuses.Length > 0)
                {
                    _chanceToApply += worldBonuses.GetChanceBonusesTotal(_chanceToApply);
                }
                
            }

            var apply = RNGService.Roll(chance);
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