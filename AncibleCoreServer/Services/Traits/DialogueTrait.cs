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
        private InteractionType _interaction = InteractionType.Talk;

        public DialogueTrait(TraitData data) : base(data)
        {
            if (data is DialogueTraitData dialogueData)
            {
                _interaction = dialogueData.Inspect ? InteractionType.Inspect : InteractionType.Talk;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            if (!_parent.Interactions.Contains(_interaction))
            {
                _parent.Interactions.Add(_interaction);
            }

            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _parent.SubscribeWithFilter<InteractWithObjectMessage>(InteractWithObject, _instanceId);
        }

        private void InteractWithObject(InteractWithObjectMessage msg)
        {
            if (msg.Type == _interaction)
            {
                this.SendMessageTo(new ClientShowDialogueMessage{Dialogue = Name, OwnerId = _parent.Id}, msg.Owner);
            }
        }

    }
}