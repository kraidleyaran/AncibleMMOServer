using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class SpawnedObjectTrait : ObjectTrait
    {
        private const string TRAIT_NAME = "Spawned Object Trait";
        private WorldObject _spawner = null;

        public SpawnedObjectTrait(WorldObject spawner)
        {
            Name = TRAIT_NAME;
            _spawner = spawner;
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _parent.SubscribeWithFilter<UpdateObjectStateMessage>(UpdateObjectState, _instanceId);
        }

        private void UpdateObjectState(UpdateObjectStateMessage msg)
        {
            if (msg.State == ObjectState.Dead)
            {
                this.SendMessageTo(new ObjectDeadMessage{Object = _parent}, _spawner);
            }
        }
    }
}