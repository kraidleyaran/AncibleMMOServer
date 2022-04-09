using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class DestroyObjectTrait : ObjectTrait
    {
        public override bool Instant => true;

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            ObjectManagerService.RemoveObjectFromWorld(_parent.Map, _parent);
        }
    }
}