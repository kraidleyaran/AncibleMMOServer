using System.Collections.Generic;
using System.Linq;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Maps;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreCommon.CommonData.WorldEvent;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;
using RogueSharp;

namespace AncibleCoreServer.Services.Maps
{
    public class WorldMap
    {
        public string Name;
        
        private Dictionary<Vector2IntData, MapTile> _tiles = new Dictionary<Vector2IntData, MapTile>();
        private Map _pathfindingMap = null;
        private PathFinder _pathFinder = null;
        private Vector2IntData _offset = new Vector2IntData();

        public WorldMap(MapData data)
        {
            Name = data.Name;
            _offset = data.Offset;
            _pathfindingMap = new Map(data.Size.X, data.Size.Y);
            
            for (var i = 0; i < data.Tiles.Length; i++)
            {
                if (!_tiles.ContainsKey(data.Tiles[i].Position))
                {
                    var tileData = data.Tiles[i];
                    var cellPos = new Vector2IntData(tileData.Position.X + data.Offset.X, tileData.Position.Y + data.Offset.Y);
                    _pathfindingMap.SetCellProperties(cellPos.X, cellPos.Y, true, true, true);
                    var cell = _pathfindingMap.GetCell(cellPos.X, cellPos.Y);
                    
                    _tiles.Add(tileData.Position, new MapTile(tileData.Position, cell));
                }
            }

            var cells = _pathfindingMap.GetAllCells().Where(c => !_tiles.ContainsKey(new Vector2IntData(c.X - _offset.X, c.Y - _offset.Y))).ToArray();
            for (var i = 0; i < cells.Length; i++)
            {
                _pathfindingMap.SetCellProperties(cells[i].X, cells[i].Y, false, false);
            }
            _pathFinder = new PathFinder(_pathfindingMap, 10);
            SubscribeToMessages();
        }

        public MapTile GetTileByPosition(Vector2IntData position)
        {
            if (_tiles.TryGetValue(position, out var tile))
            {
                return tile;
            }

            return null;
        }

        public MapTile GetTileByCell(ICell cell)
        {
            var pos = new Vector2IntData(cell.X - _offset.X, cell.Y - _offset.Y);
            return GetTileByPosition(pos);
        }

        public MapTile[] GetTilesInSquare(Vector2IntData basePosition, int distance = 1, bool pov = false)
        {
            var tile = GetTileByPosition(basePosition);
            if (tile != null)
            {
                return pov ? _pathfindingMap.ComputeFov(tile.Cell.X, tile.Cell.Y, distance, true).Select(GetTileByCell).Where(t => t != null).ToArray() : _pathfindingMap.GetCellsInSquare(tile.Cell.X, tile.Cell.Y, distance).Select(GetTileByCell).Where(t => t != null).ToArray();
            }
            return null;
        }

        public MapTile[] GetPathToTile(MapTile origin, MapTile destination)
        {
            
            var mapTiles = new List<MapTile>();
            var path = _pathFinder.ShortestPath(origin.Cell, destination.Cell);
            var pathLength = path.Length;
            var pathSteps = path.Steps.ToArray();
            for (var i = 0; i < pathLength; i++)
            {
                var tile = GetTileByCell(pathSteps[i]);
                if (tile != null)
                {
                    mapTiles.Add(tile);
                }
                else
                {
                    break;
                }
            }

            if (mapTiles.Count > 0)
            {
                if (mapTiles[0] == origin)
                {
                    mapTiles.RemoveAt(0);
                }
            }
            return mapTiles.ToArray();
        }

        public WorldEvent[] GetEventsOnTile(Vector2IntData tile)
        {
            if (_tiles.TryGetValue(tile, out var mapTile))
            {
                return mapTile.EventsOnTile.ToArray();
            }
            return new WorldEvent[0];
        }

        public WorldEvent[] GetEventsInArea(Vector2IntData origin)
        {
            var tiles = GetMapTilesInRectangle(origin, MapService.CullingBox.X, MapService.CullingBox.Y).Where(t => t.EventsOnTile.Count > 0).ToArray();
            var events = new List<WorldEvent>();
            for (var i = 0; i < tiles.Length; i++)
            {
                events.AddRange(tiles[i].EventsOnTile);
            }

            return events.ToArray();
        }

        public void SetObstacleOnTile(WorldObject obj, Vector2IntData pos)
        {
            if (_tiles.TryGetValue(pos, out var tile))
            {
                tile.ObstaclesOnTile.Add(obj);
                _pathfindingMap.SetCellProperties(tile.Cell.X, tile.Cell.Y, false, false);
            }
        }

        public Vector2IntData[] GetBlockingTilesInArea(Vector2IntData pos)
        {
            var areaTiles = GetMapTilesInRectangle(pos, MapService.CullingBox.X, MapService.CullingBox.Y);
            return areaTiles.Where(t => t.Obstacle).Select(t => t.Position).ToArray();
        }

        public MapTile[] GetMapTilesInRectangle(Vector2IntData pos, int width, int height)
        {
            var tile = GetTileByPosition(pos);
            if (tile != null)
            {
                return _pathfindingMap.GetCellsInRectangle(tile.Cell.Y - height / 2, tile.Cell.X - width / 2, width, height).Select(GetTileByCell).Where(t => t != null).ToArray();
            }
            return null;
        }

        private void SubscribeToMessages()
        {
            this.Subscribe<ResolveTickMessage>(ResolveTick);
        }

        private void ResolveTick(ResolveTickMessage msg)
        {
            var tiles = _tiles.Values.ToArray();
            for (var i = 0; i < tiles.Length; i++)
            {
                tiles[i].Resolve();
            }
        }

        public void Destroy()
        {
            this.UnsubscribeFromAllMessages();
        }
    }
}