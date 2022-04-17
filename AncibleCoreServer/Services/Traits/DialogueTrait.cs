using System.Linq;
using AncibleCoreCommon;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class DialogueTrait : ObjectTrait
    {
        public DialogueTrait(TraitData data) : base(data)
        {
            
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            if (!_parent.Interactions.Contains(InteractionType.Talk))
            {
                _parent.Interactions.Add(InteractionType.Talk);
            }
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _parent.SubscribeWithFilter<InteractWithObjectMessage>(InteractWithObject, _instanceId);
        }

        private void InteractWithObject(InteractWithObjectMessage msg)
        {
            if (msg.Type == InteractionType.Talk)
            {
                this.SendMessageTo(new ClientShowDialogueMessage{Dialogue = Name, OwnerId = _parent.Id}, msg.Owner);
            }
        }

    }
}