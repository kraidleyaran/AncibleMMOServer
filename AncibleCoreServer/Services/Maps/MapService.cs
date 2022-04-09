using System.Collections.Generic;
using System.IO;
using System.Linq;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Maps;
using AncibleCoreCommon.CommonData.WorldEvent;
using AncibleCoreServer.Services.ObjectManager;
using FileDataLib;
using MessageBusLib;

namespace AncibleCoreServer.Services.Maps
{
    public class MapService : WorldService
    {
        public static WorldMap DefaultMap { get; private set; }
        public static MapTile DefaultTile { get; private set; }
        public static Checkpoint DefaultCheckpoint { get; private set; }
        public static Vector2IntData CullingBox { get; private set; }

        public override string Name => "Map Service";

        private string _mapsPath = string.Empty;

        private static MapService _instance = null;

        private Dictionary<string, WorldMap> _maps = new Dictionary<string, WorldMap>();
        private Dictionary<string, Checkpoint> _checkpoints = new Dictionary<string, Checkpoint>();

        public MapService(string mapsPath)
        {
            _mapsPath = mapsPath;
        }

        public override void Start()
        {
            if (_instance == null)
            {
                _instance = this;
                var totalFiles = 0;
                if (Directory.Exists(_mapsPath))
                {
                    var files = Directory.GetFiles(_mapsPath, $"*.{DataExtensions.WORLD_MAP}");
                    totalFiles = files.Length;
                    for (var i = 0; i < files.Length; i++)
                    {
                        var response = FileData.LoadData<MapData>(files[i]);
                        if (response.Success)
                        {
                            if (!_maps.ContainsKey(response.Data.Name))
                            {
                                _maps.Add(response.Data.Name, new WorldMap(response.Data));
                            }
                        }
                    }
                }
                Log($"Loaded {_maps.Count} out of {totalFiles} World Maps");
                SubscribeToMessages();
                base.Start();
            }
            
        }

        public static MapTile[] GetMapTilesInArea(string map, MapTile tile, int area = 1, bool pov = false)
        {
            if (_instance._maps.TryGetValue(map, out var worldMap))
            {
                return worldMap.GetTilesInSquare(tile.Position, area, pov);
            }
            return new MapTile[0];
        }

        public static MapTile[] GetMapTilesInRectangleArea(string map, MapTile tile, int width, int height)
        {
            if (_instance._maps.TryGetValue(map, out var worldMap))
            {
                return worldMap.GetMapTilesInRectangle(tile.Position, width, height);
            }
            return new MapTile[0];
        }

        public static MapTile GetMapTileInMapByPosition(string map, Vector2IntData position)
        {
            if (_instance._maps.TryGetValue(map, out var worldMap))
            {
                return worldMap.GetTileByPosition(position);
            }
            return null;
        }

        public static MapTile[] GetPathToTileInMap(string map, Vector2IntData origin, Vector2IntData destination)
        {
            if (_instance._maps.TryGetValue(map, out var worldMap))
            {
                var originTile = worldMap.GetTileByPosition(origin);
                var destinationTile = worldMap.GetTileByPosition(destination);
                if (originTile != null && destinationTile != null)
                {
                    return worldMap.GetPathToTile(originTile, destinationTile);
                }
                return new MapTile[0];
            }
            return new MapTile[0];
        }

        public static WorldEvent[] GetEventsInAreaOnMap(string map, MapTile origin)
        {
            if (_instance._maps.TryGetValue(map, out var worldMap))
            {
                return worldMap.GetEventsInArea(origin.Position);
            }

            return new WorldEvent[0];
        }

        public static bool DoesMapExist(string map)
        {
            return _instance._maps.ContainsKey(map);
        }

        public static void SetObstacleOnMapTile(WorldObject obj, string map, Vector2IntData tile)
        {
            if (_instance._maps.TryGetValue(map, out var worldMap))
            {
                worldMap.SetObstacleOnTile(obj, tile);
            }
        }

        public static Vector2IntData[] GetBlockingTilesInAreaOfMap(Vector2IntData pos, string map)
        {
            if (_instance._maps.TryGetValue(map, out var worldMap))
            {
                return worldMap.GetBlockingTilesInArea(pos);
            }
            return new Vector2IntData[0];
        }

        public static Checkpoint GetCheckpointByName(string name)
        {
            if (_instance._checkpoints.TryGetValue(name, out var checkPoint))
            {
                return checkPoint;
            }

            return null;
        }

        public static void RegisterCheckpoint(Checkpoint checkpoint)
        {
            if (!_instance._checkpoints.ContainsKey(checkpoint.Name))
            {
                _instance._checkpoints.Add(checkpoint.Name, checkpoint);
            }
        }

        private void SubscribeToMessages()
        {
            this.Subscribe<SetDefaultTileMessage>(SetDefaultTile);
            this.Subscribe<SetDefaultCheckpointMessage>(SetDefaultCheckpoint);
            this.Subscribe<SetCullingBoxMessage>(SetCullingBox);
        }

        private void SetDefaultTile(SetDefaultTileMessage msg)
        {
            if (_maps.TryGetValue(msg.Map, out var map))
            {
                var tile = map.GetTileByPosition(msg.Position);
                if (tile != null)
                {
                    DefaultMap = map;
                    DefaultTile = tile;
                    Log($"Default map set to {DefaultMap.Name} and default tile to {tile.Position}");
                }
                else
                {
                    Log($"Could not find tile for SetDefaultTile - {msg.Position}");
                }
            }
            else
            {
                Log($"Could not find map for SetDefaultTile - {msg.Map}");
            }
        }

        private void SetDefaultCheckpoint(SetDefaultCheckpointMessage msg)
        {
            if (_checkpoints.TryGetValue(msg.Default, out var checkPoint))
            {
                DefaultCheckpoint = checkPoint;
                Log($"Defualt check point set to {msg.Default}");
            }
            else
            {
                Log($"Could not find Default Checkpoint {msg.Default}");
            }
        }

        private void SetCullingBox(SetCullingBoxMessage msg)
        {
            CullingBox = msg.Box;
            Log($"Culling box set: {CullingBox}");
        }

        public override void Stop()
        {
            var maps = _maps.Values.ToArray();
            for (var i = 0; i < maps.Length; i++)
            {
                maps[i].Destroy();
            }
            _maps.Clear();
            _maps = new Dictionary<string, WorldMap>();
            base.Stop();
        }
    }
}