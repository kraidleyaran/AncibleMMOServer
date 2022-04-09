using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class ObjectStateTrait : ObjectTrait
    {
        private ObjectState _state = ObjectState.Active;

        public ObjectStateTrait()
        {
            Name = "Object State Trait";
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _parent.SubscribeWithFilter<SetObjectStateMessage>(SetObjectState, _instanceId);
            _parent.SubscribeWithFilter<QueryObjectStateMessage>(QueryObjectState, _instanceId);
        }

        private void SetObjectState(SetObjectStateMessage msg)
        {
            if (_state != msg.State)
            {
                _state = msg.State;
                this.SendMessageTo(new UpdateObjectStateMessage{State = _state}, _parent);
            }
        }

        private void QueryObjectState(QueryObjectStateMessage msg)
        {
            msg.DoAfter(_state);
        }
    }
}