using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.Maps;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class AiWanderingTrait : ObjectTrait
    {
        private List<MapTile> _currentPath = new List<MapTile>();

        private AiState _aiState = AiState.Wandering;
        private int _wanderRange = 0;
        private MapTile _baseMapTile = null;
        private float _chanceToIdle;
        private IntNumberRange _idleTickRange = new IntNumberRange(0,1);

        private Thread _pathFindingThread = null;
        private TickTimer _idleTimer = null;

        public AiWanderingTrait(TraitData data) : base(data)
        {
            if (data is AiWanderingTraitData wanderingData)
            {
                _wanderRange = wanderingData.WanderRange;
                _idleTickRange = wanderingData.IdleTickRange;
                _chanceToIdle = wanderingData.ChanceToIdle;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            _baseMapTile = _parent.Tile;
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            this.Subscribe<WorldTickMessage>(WorldTick);

            _parent.SubscribeWithFilter<UpdateAiStateMessage>(UpdateAiState);
            _parent.SubscribeWithFilter<UpdateTileMessage>(UpdateTile);
        }

        private void UpdateAiState(UpdateAiStateMessage msg)
        {
            _aiState = msg.State;
            if (_aiState != AiState.Wandering)
            {
                _currentPath.Clear();
                if (_idleTimer != null)
                {
                    _idleTimer.Destroy();
                    _idleTimer = null;
                }
            }
        }

        private void UpdateTile(UpdateTileMessage msg)
        {
            if (_aiState == AiState.Wandering)
            {
                if (_currentPath.Count > 0)
                {
                    if (msg.Tile == _currentPath[0])
                    {
                        _currentPath.RemoveAt(0);
                    }

                    if (_currentPath.Count > 0)
                    {
                        var direction = _parent.Tile.Position.Direction(_currentPath[0].Position);
                        this.SendMessageTo(new SetDirectionMessage{Direction = direction}, _parent);
                    }
                    else
                    {
                        this.SendMessageTo(new SetDirectionMessage{Direction = Vector2IntData.zero}, _parent);
                    }
                }
                
            }
        }

        private void WorldTick(WorldTickMessage msg)
        {
            if (_aiState == AiState.Wandering)
            {
                if (_currentPath.Count <= 0 && _pathFindingThread == null && _idleTimer == null)
                {
                    var idle = RNGService.Roll(_chanceToIdle);
                    if (idle)
                    {
                        _idleTimer = new TickTimer(_idleTickRange.GenerateRandomNumber(RNGService.RANDOM), () => { _idleTimer = null;});
                    }
                    else
                    {
                        var tilesInFence = MapService.GetMapTilesInArea(_parent.Map, _baseMapTile, _wanderRange + 1, true).ToList();
                        tilesInFence.RemoveAll(t => t == _parent.Tile);
                        if (tilesInFence.Count > 0)
                        {
                            _pathFindingThread = new Thread(() =>
                            {
                                if (!_parent.BeingDestroy)
                                {
                                    var destination = tilesInFence.Count > 1 ? tilesInFence[RNGService.RollRange(0, tilesInFence.Count)] : tilesInFence[0];
                                    _currentPath = MapService.GetPathToTileInMap(_parent.Map, _parent.Tile.Position, destination.Position).ToList();
                                    var direction = _parent.Tile.Position.Direction(_currentPath[0].Position);
                                    this.SendMessageTo(new SetDirectionMessage { Direction = direction }, _parent);
                                }
                                _pathFindingThread = null;
                            });
                            _pathFindingThread.Start();
                        }
                    }

                }

            }
        }
    }
}