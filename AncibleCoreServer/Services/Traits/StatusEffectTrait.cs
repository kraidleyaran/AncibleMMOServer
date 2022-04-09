using AncibleCoreCommon;
using AncibleCoreCommon.CommonData.Client;
using AncibleCoreCommon.CommonData.Combat;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreCommon.CommonData.WorldEvent;
using AncibleCoreServer.Services.Combat;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class StatusEffectTrait : ObjectTrait
    {
        private int _length = 0;
        private StatusEffectType _type;

        private TickTimer _timer = null;

        public StatusEffectTrait(TraitData data) : base(data)
        {
            if (data is StatusEffectTraitData effectData)
            {
                _length = effectData.Length;
                _type = effectData.EffectType;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            _timer = new TickTimer(_length, StatusCompleted);
            WorldObject parentObj = null;
            if (_sender != null)
            {
                this.SendMessageTo(new QueryWorldObjectMessage{DoAfter = obj => parentObj = obj}, _sender);
            }
            this.SendMessageTo(new ApplyStatusEffectMessage{Type = _type, Owner = parentObj}, _parent);
            var eventMessage = string.Empty;
            eventMessage = $"{_parent.DisplayName} has been {_type.ToPastTenseEffectString()}";
            if (parentObj != null)
            {
                eventMessage = $"{eventMessage} by {parentObj.DisplayName}!";
            }
            else
            {
                eventMessage = $"{eventMessage}!";
            }
            _parent.Tile.EventsOnTile.Add(new StatusEffectWorldEvent{OwnerId = parentObj?.Id ?? string.Empty, TargetId = _parent.Id, Effect = _type, Text = eventMessage});
            SubscribeToMessages();
        }

        private void StatusCompleted()
        {
            this.SendMessageTo(new RemoveTraitFromObjectMessage{Trait = this}, _parent);
        }

        private void SubscribeToMessages()
        {
            _parent.SubscribeWithFilter<QueryStatusEffectsMessage>(QueryStatusEffects, _instanceId);
            _parent.SubscribeWithFilter<StatusEffectCheckMessage>(StatusEffectCheck, _instanceId);
            _parent.SubscribeWithFilter<QueryClientStatusEffectsMessage>(QueryClientStatusEffects, _instanceId);
            if (_type == StatusEffectType.Sleep)
            {
                _parent.SubscribeWithFilter<TakeDamageMessage>(TakeDamage, _instanceId);
            }
        }

        private void StatusEffectCheck(StatusEffectCheckMessage msg)
        {
            if (msg.Type == _type)
            {
                _timer.Destroy();
                _timer = null;
                StatusCompleted();
            }
        }

        private void QueryClientStatusEffects(QueryClientStatusEffectsMessage msg)
        {
            msg.DoAfter.Invoke(new ClientStatusEffectData{CurrentTick = _timer?.TickCount ?? 0, MaxTick = _length, Type = _type});
        }

        private void QueryStatusEffects(QueryStatusEffectsMessage msg)
        {
            msg.DoAfter.Invoke(_type);
        }

        private void TakeDamage(TakeDamageMessage msg)
        {
            var wakeUp = RNGService.Roll(CombatService.ChanceToWakeFromDamage);
            if (wakeUp)
            {
                StatusCompleted();
            }
        }
    }
}