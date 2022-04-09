using System;
using System.Collections.Generic;
using AncibleCoreCommon;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Combat;
using AncibleCoreCommon.CommonData.PlayerEvent;
using AncibleCoreServer.Services.Maps;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class PlayerMovementTrait : ObjectTrait
    {
        public const string TRAIT_NAME = "Client Movement Trait";

        private ObjectState _objectState = ObjectState.Active;
        private MapTile _nextTile = null;
        private int _ticksToMove;
        private int _tickCount;
        private string _playerId = string.Empty;

        private List<Vector2IntData> _moveCommandQueue = new List<Vector2IntData>();

        private Checkpoint _checkpoint = null;

        public PlayerMovementTrait(string playerId, Checkpoint checkpoint, int ticksToMove = 1)
        {
            Name = TRAIT_NAME;
            _playerId = playerId;
            _ticksToMove = ticksToMove;
            _checkpoint = checkpoint;
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            this.Subscribe<WorldTickMessage>(WorldTick);
            this.SubscribeWithFilter<ClientMoveCommandMessage>(ClientMoveCommand, _playerId);

            _parent.SubscribeWithFilter<UpdateObjectStateMessage>(UpdateObjectState, _instanceId);
            _parent.SubscribeWithFilter<RespawnPlayerMessage>(RespawnPlayer, _instanceId);
            _parent.SubscribeWithFilter<ApplyStatusEffectMessage>(ApplyStatusEffect, _instanceId);
            _parent.SubscribeWithFilter<SetCurrentCheckpointMessage>(SetCurrentCheckpoint, _instanceId);
            _parent.SubscribeWithFilter<QueryCurrentCheckpointMessage>(QueryCurrentCheckpoint, _instanceId);
            _parent.SubscribeWithFilter<TransferToMapMessage>(TransferToMap, _instanceId);
        }

        private void ClientMoveCommand(ClientMoveCommandMessage msg)
        {
            var statusEffects = new List<StatusEffectType>();
            this.SendMessageTo(new QueryStatusEffectsMessage
            {
                DoAfter = effect =>
                {
                    statusEffects.Add(effect);
                }
            }, _parent);
            if (statusEffects.Contains(StatusEffectType.Daze) || statusEffects.Contains(StatusEffectType.Sleep) || statusEffects.Contains(StatusEffectType.Root))
            {
                this.SendMessageTo(new ClientMovementResponseMessage { Success = false, Position = _parent.Tile.Position }, _parent);
            }
            else
            {
                _moveCommandQueue.Add(msg.Direction);
            }
            
        }

        private void WorldTick(WorldTickMessage msg)
        {
            if (_objectState != ObjectState.Dead && _objectState != ObjectState.Moving)
            {
                var statusEffects = new List<StatusEffectType>();
                this.SendMessageTo(new QueryStatusEffectsMessage{DoAfter = effect =>
                {
                    statusEffects.Add(effect);
                }}, _parent);
                if (!(statusEffects.Contains(StatusEffectType.Daze) || statusEffects.Contains(StatusEffectType.Sleep) || statusEffects.Contains(StatusEffectType.Root)))
                {
                    if (_nextTile != null)
                    {
                        this.SendMessageTo(new SetObjectStateMessage { State = ObjectState.Moving }, _parent);
                    }
                    else if (_moveCommandQueue.Count > 0)
                    {
                        var direction = _moveCommandQueue[0];
                        _moveCommandQueue.RemoveAt(0);
                        _nextTile = MapService.GetMapTileInMapByPosition(_parent.Map, direction + _parent.Tile.Position);
                        var confirmMovementMsg = new ClientMovementResponseMessage();
                        if (_nextTile != null && !_nextTile.Obstacle)
                        {
                            confirmMovementMsg.Success = true;
                            confirmMovementMsg.Position = _nextTile.Position;
                        }
                        else
                        {
                            _nextTile = null;
                            _moveCommandQueue.Clear();
                            this.SendMessageTo(new SetObjectStateMessage { State = ObjectState.Active }, _parent);
                            confirmMovementMsg.Success = false;
                            confirmMovementMsg.Position = _parent.Tile.Position;
                        }
                        this.SendMessageTo(confirmMovementMsg, _parent);
                    }
                }

            }
            else if (_objectState == ObjectState.Moving)
            {
                if (_nextTile != null)
                {
                    _tickCount++;
                    if (_tickCount > _ticksToMove)
                    {
                        _tickCount = 0;
                        _parent.Tile.ObjectsOnTile.Remove(_parent);
                        _parent.Tile = _nextTile;
                        _parent.Tile.ObjectsOnTile.Add(_parent);
                        this.SendMessageTo(new UpdateTileMessage { Tile = _parent.Tile }, _parent);
                        if (_nextTile.TileEvents.Count > 0)
                        {
                            _nextTile.ApplyEvents(_parent);
                        }
                        //Console.WriteLine($"Position: {_parent.Tile.Position}");
                        if (_moveCommandQueue.Count > 0)
                        {
                            var direction = _moveCommandQueue[0];
                            _moveCommandQueue.RemoveAt(0);
                            _nextTile = MapService.GetMapTileInMapByPosition(_parent.Map, direction + _parent.Tile.Position);
                            var confirmMovementMsg = new ClientMovementResponseMessage();
                            if (_nextTile != null && !_nextTile.Obstacle)
                            {
                                confirmMovementMsg.Success = true;
                                confirmMovementMsg.Position = _nextTile.Position;
                            }
                            else
                            {
                                _nextTile = null;
                                _moveCommandQueue.Clear();
                                confirmMovementMsg.Success = false;
                                confirmMovementMsg.Position = _parent.Tile.Position;
                                this.SendMessageTo(new SetObjectStateMessage { State = ObjectState.Active }, _parent);
                            }
                            this.SendMessageTo(confirmMovementMsg, _parent);
                        }
                        else
                        {
                            _nextTile = null;
                            this.SendMessageTo(new SetObjectStateMessage { State = ObjectState.Active }, _parent);
                        }
                    }
                }
                else
                {
                    _tickCount = 0;
                    this.SendMessageTo(new SetObjectStateMessage { State = ObjectState.Active }, _parent);
                }
            }
        }

        private void UpdateObjectState(UpdateObjectStateMessage msg)
        {
            _objectState = msg.State;
            if (_objectState == ObjectState.Dead)
            {
                _tickCount = 0;
                _moveCommandQueue.Clear();
                _nextTile = null;
            }
        }

        private void RespawnPlayer(RespawnPlayerMessage msg)
        {
            _parent.Map = _checkpoint.Map;
            _parent.Tile = _checkpoint.Tile;
        }

        private void ApplyStatusEffect(ApplyStatusEffectMessage msg)
        {
            switch (msg.Type)
            {
                case StatusEffectType.Daze:
                case StatusEffectType.Root:
                case StatusEffectType.Sleep:
                    if (_tickCount > 0)
                    {
                        _tickCount = 0;
                    }

                    if (_nextTile != null)
                    {
                        _nextTile = null;
                        _moveCommandQueue.Clear();
                        this.SendMessageTo(new ClientMovementResponseMessage { Success = false, Position = _parent.Tile.Position }, _parent);
                    }
                    break;
            }
        }

        private void SetCurrentCheckpoint(SetCurrentCheckpointMessage msg)
        {
            _checkpoint = msg.Checkpoint;
            this.SendMessageTo(new RegisterPlayerEventMessage{Event = new PlayerCheckpointEvent{Checkpoint = _checkpoint.Name}}, _parent);
        }

        private void QueryCurrentCheckpoint(QueryCurrentCheckpointMessage msg)
        {
            msg.DoAfter.Invoke(_checkpoint);
        }

        private void TransferToMap(TransferToMapMessage msg)
        {
            _moveCommandQueue.Clear();
            _tickCount = 0;
            _parent.Tile.ObjectsOnTile.Remove(_parent);
            _parent.Map = msg.Map;
            _parent.Tile = msg.Tile;
            _parent.Tile.ObjectsOnTile.Add(_parent);
            this.SendMessageTo(new ClientTransferToMapMessage{Map = _parent.Map, Tile = _parent.Tile.Position},  _parent);
        }
    }
}