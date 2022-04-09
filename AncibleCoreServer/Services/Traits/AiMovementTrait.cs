using System.Collections.Generic;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Combat;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.Maps;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class AiMovementTrait : ObjectTrait
    {
        private int _ticksToMove = 1;
        private int _tickCount = 0;

        private Vector2IntData _direction = Vector2IntData.zero;

        private ObjectState _objState = ObjectState.Active;

        public AiMovementTrait(TraitData data) : base(data)
        {
            if (data is AiMovementTraitData movementData)
            {
                _ticksToMove = movementData.TicksToMove;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            this.Subscribe<WorldTickMessage>(WorldTick);

            _parent.SubscribeWithFilter<SetDirectionMessage>(SetDirection, _instanceId);
            _parent.SubscribeWithFilter<UpdateObjectStateMessage>(UpdateObjectState, _instanceId);
        }

        private void SetDirection(SetDirectionMessage msg)
        {
            _direction = msg.Direction;
            if (_direction == Vector2IntData.zero)
            {
                _tickCount = 0;
                this.SendMessageTo(new SetObjectStateMessage { State = ObjectState.Active }, _parent);
            }
        }

        private void WorldTick(WorldTickMessage msg)
        {
            var statusEffects = new List<StatusEffectType>();
            this.SendMessageTo(new QueryStatusEffectsMessage{DoAfter = type => statusEffects.Add(type)}, _parent);
            if (!(statusEffects.Contains(StatusEffectType.Daze) || statusEffects.Contains(StatusEffectType.Root) || statusEffects.Contains(StatusEffectType.Sleep)))
            {
                if (_objState == ObjectState.Active)
                {
                    if (_direction != Vector2IntData.zero)
                    {
                        this.SendMessageTo(new SetObjectStateMessage { State = ObjectState.Moving }, _parent);
                    }
                }
                else if (_objState == ObjectState.Moving)
                {
                    _tickCount++;
                    if (_tickCount >= _ticksToMove)
                    {
                        _tickCount = 0;
                        var tile = MapService.GetMapTileInMapByPosition(_parent.Map, _parent.Tile.Position + _direction);
                        if (tile != null)
                        {
                            _parent.Tile.ObjectsOnTile.Remove(_parent);
                            _parent.Tile = tile;
                            _parent.Tile.ObjectsOnTile.Add(_parent);
                            this.SendMessageTo(new UpdateTileMessage { Tile = tile }, _parent);
                        }

                    }

                }
            }
        }


        private void UpdateObjectState(UpdateObjectStateMessage msg)
        {
            _objState = msg.State;
        }
    }
}