using AncibleCoreCommon.CommonData.Ability;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreCommon.CommonData.WorldEvent;
using AncibleCoreServer.Services.Ability;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class CastingTrait : ObjectTrait
    {
        private TickTimer _currentCast = null;
        private string _currentCastName = string.Empty;
        private ObjectState _state = ObjectState.Active;
        private TickTimer _globalCooldown = null;

        public CastingTrait()
        {
            Name = "Casting Trait";
        }

        public CastingTrait(TraitData data) : base(data)
        {

        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _parent.SubscribeWithFilter<CastCommandMessage>(CastCommand, _instanceId);
            _parent.SubscribeWithFilter<UpdateObjectStateMessage>(UpdateObjectState, _instanceId);
            _parent.SubscribeWithFilter<CancelCastMessage>(CancelCast, _instanceId);
            _parent.SubscribeWithFilter<TriggerGlobalCooldownMessage>(TriggerGlobalCooldown, _instanceId);
            _parent.SubscribeWithFilter<QueryGlobalCooldownMessage>(QueryGlobalCooldown, _instanceId);
            _parent.SubscribeWithFilter<ApplyStatusEffectMessage>(ApplyStatusEffect, _instanceId);
        }

        private void CastCommand(CastCommandMessage msg)
        {
            if (_globalCooldown == null && _currentCast == null && (_state == ObjectState.Active || _state == ObjectState.Interaction))
            {
                this.SendMessageTo(TriggerGlobalCooldownMessage.INSTANCE, _parent);
                _parent.Tile.EventsOnTile.Add(new CastWorldEvent{OwnerId = _parent.Id, Length = msg.Time, Ability = msg.Name});
                _currentCast = new TickTimer(msg.Time, msg.DoAfter, msg.Loop, () =>
                {
                    _currentCast.Destroy();
                    _currentCast = null;
                    this.SendMessageTo(new SetObjectStateMessage{State = ObjectState.Active}, _parent);
                });
                _currentCastName = msg.Name;
                msg.OnSuccess?.Invoke(true);
                this.SendMessageTo(new SetObjectStateMessage{State = ObjectState.Casting},  _parent);
            }
            else
            {
                msg.OnSuccess?.Invoke(false);
            }
        }

        private void UpdateObjectState(UpdateObjectStateMessage msg)
        {
            _state = msg.State;
            if ((_state == ObjectState.Moving || _state == ObjectState.Dead) && _currentCast != null)
            {
                _currentCast.Destroy();
                _currentCast = null;
                this.SendMessageTo(CastCancelledMessage.INSTANCE, _parent);
            }
        }

        private void CancelCast(CancelCastMessage msg)
        {
            if (_state == ObjectState.Casting && _currentCast != null)
            {
                _currentCast.Destroy();
                _parent.Tile.EventsOnTile.Add(new CancelCastWorldEvent { OwnerId = _parent.Id});
                _currentCast = null;
                this.SendMessageTo(CastCancelledMessage.INSTANCE, _parent);
            }
        }

        private void QueryGlobalCooldown(QueryGlobalCooldownMessage msg)
        {
            msg.DoAfter.Invoke(_globalCooldown?.TickCount ?? 0);
        }

        private void TriggerGlobalCooldown(TriggerGlobalCooldownMessage msg)
        {
            if (_globalCooldown != null)
            {
                _globalCooldown.Destroy();
                _globalCooldown = null;
            }

            _globalCooldown = new TickTimer(AbilityService.GlobalCooldown, () =>
            {
                _globalCooldown.Destroy();
                _globalCooldown = null;
            });
        }

        private void ApplyStatusEffect(ApplyStatusEffectMessage msg)
        {
            switch (msg.Type)
            {
                case AncibleCoreCommon.CommonData.Combat.StatusEffectType.Daze:
                case AncibleCoreCommon.CommonData.Combat.StatusEffectType.Pacify:
                case AncibleCoreCommon.CommonData.Combat.StatusEffectType.Sleep:
                    if (_state == ObjectState.Casting && _currentCast != null)
                    {
                        this.SendMessageTo(CancelCastMessage.INSTANCE, _parent);
                    }
                    break;
            }
        }
    }
}