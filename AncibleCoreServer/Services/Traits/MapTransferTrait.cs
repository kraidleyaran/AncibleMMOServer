using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.Maps.TileEvents;
using AncibleCoreServer.Services.ObjectManager;

namespace AncibleCoreServer.Services.Traits
{
    public class MapTransferTrait : ObjectTrait
    {
        public override bool Instant => true;

        private string _map = string.Empty;
        private Vector2IntData _tile = Vector2IntData.zero;

        public MapTransferTrait(TraitData data) : base(data)
        {
            if (data is MapTransferTraitData transferData)
            {
                _map = transferData.Map;
                _tile = transferData.Position;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            _parent.Tile.TileEvents.Add(new TransferTileEvent(_map, _tile));
        }
    }
}