using System.Linq;
using AncibleCoreCommon;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class HealerTrait : ObjectTrait
    {
        private string[] _applyOnInteract = new string[0];

        public HealerTrait(TraitData data) : base(data)
        {
            if (data is HealerTraitData healerData)
            {
                _applyOnInteract = healerData.ApplyOnInteract;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            if (!_parent.Interactions.Contains(InteractionType.Heal))
            {
                _parent.Interactions.Add(InteractionType.Heal);
            }
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _parent.SubscribeWithFilter<InteractWithObjectMessage>(InteractWithObject, _instanceId);
        }

        private void InteractWithObject(InteractWithObjectMessage msg)
        {
            if (msg.Type == InteractionType.Heal)
            {
                _parent.SendMessageTo(FullHealMessage.INSTANCE, msg.Owner);
                var traits = _applyOnInteract.Select(TraitService.GetTrait).Where(t => t != null).ToArray();
                if (traits.Length > 0)
                {
                    var addTraitToObjMsg = new AddTraitToObjectMessage();
                    for (var i = 0; i < traits.Length; i++)
                    {
                        addTraitToObjMsg.Trait = traits[i];
                        _parent.SendMessageTo(addTraitToObjMsg, msg.Owner);
                    }
                }
                _parent.SendMessageTo(new InteractionFinishedMessage { Object = _parent }, msg.Owner);
            }
        }

    }
}