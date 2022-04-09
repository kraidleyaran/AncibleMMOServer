using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class AiStateTrait : ObjectTrait
    {
        private AiState _aiState = AiState.Wandering;

        public AiStateTrait(TraitData data) : base(data)
        {

        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _parent.SubscribeWithFilter<SetAiStateMessage>(SetAiState, _instanceId);
            _parent.SubscribeWithFilter<QueryAiStateMessage>(QueryAiState, _instanceId);
        }

        private void SetAiState(SetAiStateMessage msg)
        {
            if (_aiState != msg.State)
            {
                _aiState = msg.State;
                this.SendMessageTo(new UpdateAiStateMessage{State = _aiState}, _parent);
            }
        }

        private void QueryAiState(QueryAiStateMessage msg)
        {
            msg.DoAfter.Invoke(_aiState);
        }
    }
}