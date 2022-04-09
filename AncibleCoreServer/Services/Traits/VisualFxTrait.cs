using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreCommon.CommonData.WorldEvent;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class VisualFxTrait : ObjectTrait
    {
        public override bool Instant => true;

        private string _visualFx = string.Empty;

        public VisualFxTrait(TraitData data) : base(data)
        {
            if (data is VisualFxTraitData fxData)
            {
                _visualFx = fxData.VisualFx;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            owner.Tile.EventsOnTile.Add(new VisualFxWorldEvent
            {
                VisualFx = _visualFx,
                OwnerId = owner.Id,
                OverridePosition = owner.Tile.Position,
            });
        }
    }
}