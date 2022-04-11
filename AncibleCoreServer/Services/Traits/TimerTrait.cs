using System.Collections.Generic;
using System.Linq;
using AncibleCoreCommon.CommonData.Client;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreCommon.CommonData.WorldEvent;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class TimerTrait : ObjectTrait
    {
        private int _timerTicks = 0;
        private string[] _applyOnStart;
        private string[] _applyOnEnd;
        private string _status = string.Empty;
        private bool _damageInterruptable = false;
        private bool _show = false;
        private ObjectIconType _showType = ObjectIconType.Neutral;


        private ObjectTrait[] _appliedTraits = new ObjectTrait[0];
        private TickTimer _timer = null;

        public TimerTrait(TraitData data) : base(data)
        {
            if (data is TimerTraitData timerData)
            {
                _timerTicks = timerData.TimerTicks;
                _applyOnStart = timerData.ApplyOnStart;
                _applyOnEnd = timerData.ApplyOnEnd;
                _status = timerData.Status;
                _damageInterruptable = timerData.DamageInterruptable;
                _show = timerData.Display;
                _showType = timerData.IconType;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            if (_applyOnStart.Length > 0)
            {
                var traits = _applyOnStart.Select(TraitService.GetTrait).Where(t => t != null).ToArray();
                if (traits.Length > 0)
                {
                    var appliedTraits = new List<ObjectTrait>();
                    var addTraitToObjMsg = new AddTraitToObjectMessage();
                    for (var i = 0; i < traits.Length; i++)
                    {
                        addTraitToObjMsg.Trait = traits[i];
                        _sender.SendMessageTo(addTraitToObjMsg, _parent);
                        if (!traits[i].Instant)
                        {
                            appliedTraits.Add(traits[i]);
                        }
                    }

                    _appliedTraits = appliedTraits.ToArray();
                }
            }
            _timer = new TickTimer(_timerTicks, OnTimerCompleted);
            if (!string.IsNullOrEmpty(_status))
            {
                WorldObject parentObj = null;
                this.SendMessageTo(new QueryWorldObjectMessage{DoAfter = obj => parentObj = obj}, _sender);
                var eventMessage = $"{_parent.DisplayName} has been {_status}";
                eventMessage = parentObj != null ? $"{eventMessage} by {parentObj.DisplayName}!" : $"{eventMessage}!";
                _parent.Tile.EventsOnTile.Add(new CustomStatusWorldEvent{Status = _status, TargetId = _parent.Id, OwnerId = parentObj?.Id ?? string.Empty, Text = eventMessage});
            }
            SubscribeToMessages();
        }

        private void OnTimerCompleted()
        {
            if (_appliedTraits.Length > 0)
            {
                var removeTraitFromObjMsg = new RemoveTraitFromObjectMessage();
                for (var i = 0; i < _appliedTraits.Length; i++)
                {
                    removeTraitFromObjMsg.Trait = _appliedTraits[i];
                    this.SendMessageTo(removeTraitFromObjMsg, _parent);
                }
                _appliedTraits = new ObjectTrait[0];
            }
            if (_applyOnEnd.Length > 0)
            {
                var traits = _applyOnEnd.Select(TraitService.GetTrait).Where(t => t != null).ToArray();
                if (traits.Length > 0)
                {
                    var addTraitToObjMsg = new AddTraitToObjectMessage();

                    for (var i = 0; i < traits.Length; i++)
                    {
                        addTraitToObjMsg.Trait = traits[i];
                        _sender.SendMessageTo(addTraitToObjMsg, _parent);
                    }
                }

            }
            _timer.Destroy();
            _timer = null;
            this.SendMessageTo(new RemoveTraitFromObjectMessage{Trait = this}, _parent);
        }

        private void SubscribeToMessages()
        {
            _parent.SubscribeWithFilter<RefreshTimerMessage>(RefreshTimer, _instanceId);
            if (_show)
            {
                _parent.SubscribeWithFilter<QueryClientIconDataMessage>(QueryClientIconData, _instanceId);
            }
            if (_damageInterruptable)
            {
                _parent.SubscribeWithFilter<TakeDamageMessage>(TakeDamage, _instanceId);
            }
        }

        private void RefreshTimer(RefreshTimerMessage msg)
        {
            if (msg.Timer == Name)
            {
                _timer?.Reset();
                _parent.Update = true;
            }
            
        }

        private void TakeDamage(TakeDamageMessage msg)
        {
            this.SendMessageTo(new RemoveTraitFromObjectMessage{Trait = this}, _parent);
        }

        private void QueryClientIconData(QueryClientIconDataMessage msg)
        {
            if (_timer != null)
            {
                msg.DoAfter.Invoke(new ClientObjectIconData {Title = _status, Icon = Name, Id = _instanceId, MaxTicks = _timerTicks, Ticks = _timer.TickCount, Type =  _showType});
            }
        }

        public override void Destroy()
        {
            if (_appliedTraits.Length > 0)
            {
                var removeTraitFromObjMsg = new RemoveTraitFromObjectMessage();
                for (var i = 0; i < _appliedTraits.Length; i++)
                {
                    removeTraitFromObjMsg.Trait = _appliedTraits[i];
                    _sender.SendMessageTo(removeTraitFromObjMsg, _parent);
                }
                _appliedTraits = null;
            }

            if (_timer != null)
            {
                _timer.Destroy();
                _timer = null;
            }

            _applyOnStart = null;
            _applyOnEnd = null;
            base.Destroy();
        }
    }
}