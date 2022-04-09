using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.Maps;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class CheckpointTrait : ObjectTrait
    {
        private Vector2IntData _relativePosition = Vector2IntData.zero;

        private Checkpoint _checkpoint = null;

        public CheckpointTrait(TraitData data) : base(data)
        {
            if (data is CheckpointTraitData checkpointData)
            {
                _relativePosition = checkpointData.RelativePosition;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            var mapTile = MapService.GetMapTileInMapByPosition(_parent.Map, _parent.Tile.Position + _relativePosition);
            if (mapTile != null)
            {
                if (!_parent.Interactions.Contains(InteractionType.Checkpoint))
                {
                    _parent.Interactions.Add(InteractionType.Checkpoint);
                }
                _checkpoint = new Checkpoint {Name = _parent.DisplayName ,Map = _parent.Map, Tile = mapTile};
                MapService.RegisterCheckpoint(_checkpoint);
                SubscribeToMessages();
            }
            

        }

        private void SubscribeToMessages()
        {
            if (_checkpoint != null)
            {
                _parent.SubscribeWithFilter<InteractWithObjectMessage>(InteractWithObject, _instanceId);
            }
        }

        private void InteractWithObject(InteractWithObjectMessage msg)
        {
            if (msg.Type == InteractionType.Checkpoint)
            {
                this.SendMessageTo(new SetCurrentCheckpointMessage{Checkpoint = _checkpoint}, msg.Owner);
                this.SendMessageTo(new InteractionFinishedMessage{Object = _parent}, msg.Owner);
            }
        }
    }
}