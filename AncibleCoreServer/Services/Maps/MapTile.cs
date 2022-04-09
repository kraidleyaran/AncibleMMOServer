using System.Collections.Generic;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.WorldEvent;
using AncibleCoreServer.Services.Maps.TileEvents;
using AncibleCoreServer.Services.ObjectManager;
using RogueSharp;

namespace AncibleCoreServer.Services.Maps
{
    public class MapTile
    {
        public Vector2IntData Position;
        public List<WorldObject> ObjectsOnTile;
        public ICell Cell;
        public bool Obstacle => ObstaclesOnTile.Count > 0;
        public List<WorldObject> ObstaclesOnTile;
        public List<WorldEvent> EventsOnTile;
        public List<TileEvent> TileEvents;


        public MapTile(Vector2IntData position, ICell cell)
        {
            Position = position;
            Cell = cell;
            ObjectsOnTile = new List<WorldObject>();
            ObstaclesOnTile = new List<WorldObject>();
            EventsOnTile = new List<WorldEvent>();
            TileEvents = new List<TileEvent>();
        }

        public void ApplyEvents(WorldObject obj)
        {
            for (var i = 0; i < TileEvents.Count; i++)
            {
                TileEvents[i].Apply(obj);
            }
        }

        public void Resolve()
        {
            EventsOnTile.Clear();
            EventsOnTile = new List<WorldEvent>();
        }
    }
}