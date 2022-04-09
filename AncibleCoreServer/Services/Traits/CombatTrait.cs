using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class CombatTrait : ObjectTrait
    {
        private CombatAlignment _alignment = CombatAlignment.Neutral;

        public CombatTrait(TraitData data)
        {
            if (data is CombatTraitData combatData)
            {
                _alignment = combatData.Alignment;
            }
        }

        public CombatTrait(CombatAlignment alignment)
        {
            Name = "Custom Combat Alignment Trait";
            _alignment = alignment;
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _parent.SubscribeWithFilter<QueryCombatAlignmentMessage>(QueryCombatAlignment, _instanceId);
            _parent.SubscribeWithFilter<RefreshObjectStateMessage>(RefreshObjectState, _instanceId);
            _parent.SubscribeWithFilter<QueryClientObjectDataMessage>(QueryClientObjectData, _instanceId);
        }

        private void QueryCombatAlignment(QueryCombatAlignmentMessage msg)
        {
            msg.DoAfter.Invoke(_alignment);
        }

        private void RefreshObjectState(RefreshObjectStateMessage msg)
        {
            this.SendMessageTo(new UpdateCombatAlignmentMessage{Alignment = _alignment}, _parent);
        }

        private void QueryClientObjectData(QueryClientObjectDataMessage msg)
        {
            msg.Data.Alignment = _alignment;
        }
    }
}