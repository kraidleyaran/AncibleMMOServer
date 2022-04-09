using AncibleCoreCommon.CommonData;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Maps.TileEvents
{
    public class TransferTileEvent : TileEvent
    {
        private Vector2IntData _tile = Vector2IntData.zero;
        private string _map = string.Empty;

        public TransferTileEvent()
        {

        }

        public TransferTileEvent(string map, Vector2IntData position)
        {
            _map = map;
            _tile = position;
        }

        public override void Apply(WorldObject obj)
        {
            var mapTile = MapService.GetMapTileInMapByPosition(_map, _tile);
            this.SendMessageTo(new TransferToMapMessage{Map = _map, Tile = mapTile}, obj);
        }
    }
}