using AncibleCoreCommon.CommonData.Ability;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class RemoveResourceTrait : ObjectTrait
    {
        private ResourceType _resource;
        private int _amount;

        public RemoveResourceTrait(TraitData data) : base(data)
        {
            if (data is RemoveResourceTraitData resourceData)
            {
                _resource = resourceData.Resource;
                _amount = resourceData.Amount;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            this.SendMessageTo(new RemoveResourceMessage{Type = _resource, Amount = _amount}, _parent);
        }
    }
}