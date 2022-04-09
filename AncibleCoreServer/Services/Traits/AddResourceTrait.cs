using AncibleCoreCommon.CommonData.Ability;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreCommon.CommonData.WorldEvent;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class AddResourceTrait : ObjectTrait
    {
        public override bool Instant => true;

        private ResourceType _resource;
        private int _amount;

        public AddResourceTrait(TraitData data) : base(data)
        {
            if (data is AddResourceTraitData resourceData)
            {
                _resource = resourceData.Resource;
                _amount = resourceData.Amount;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            WorldObject parentObj = null;
            this.SendMessageTo(new QueryWorldObjectMessage{DoAfter = obj => parentObj = obj}, _sender);
            _sender.SendMessageTo(new AddResourceMessage{Amount = _amount, Type = _resource}, _parent);
            var eventText = $"{_parent.DisplayName} gains {_resource} {_amount}";
            if (parentObj != null)
            {
                eventText = $"{eventText} from {parentObj.DisplayName}";
            }
            _parent.Tile.EventsOnTile.Add(new ResourceWorldEvent{OwnerId = parentObj?.Id ?? string.Empty, TargetId = _parent.Id,Amount = _amount, Resource = _resource, Text = eventText});
        }
    }
}