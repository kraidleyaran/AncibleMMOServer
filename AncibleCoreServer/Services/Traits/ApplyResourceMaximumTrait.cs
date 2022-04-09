using AncibleCoreCommon.CommonData.Ability;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class ApplyResourceMaximumTrait : ObjectTrait
    {
        public override bool Instant => _permanent;

        private ResourceType _resource;
        private int _amount;
        private bool _permanent;

        public ApplyResourceMaximumTrait(TraitData data) : base(data)
        {
            if (data is ApplyResourceMaximumTraitData resourceData)
            {
                _resource = resourceData.Resource;
                _amount = resourceData.Amount;
                _permanent = resourceData.Permanent;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            this.SendMessageTo(new ApplyMaximumResourceMessage{Amount = _amount, Type = _resource, Permanent = _permanent}, _parent);
        }

        public override void ApplyRemoval(WorldObject owner)
        {
            if (_permanent)
            {
                this.SendMessageTo(new ReduceMaximumResourceMessage{Amount = _amount, Permanent = true, Type = _resource}, _parent);
            }
            base.ApplyRemoval(owner);
        }

        public override void Destroy()
        {
            if (!_permanent)
            {
                this.SendMessageTo(new ReduceMaximumResourceMessage { Amount = _amount, Type = _resource, Permanent = false }, _parent);
            }
            base.Destroy();
        }
    }
}