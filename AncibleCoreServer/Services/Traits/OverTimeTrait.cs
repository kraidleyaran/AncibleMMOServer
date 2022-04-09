using System.Linq;
using AncibleCoreCommon.CommonData.Client;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreCommon.CommonData.WorldEvent;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class OverTimeTrait : ObjectTrait
    {
        private int _timerTicks = 0;
        private int _loops = 0;
        private string[] _applyOnTimerFinished;
        private bool _applyOnStart = false;
        private string _status = string.Empty;
        private bool _damageInterruptable = false;
        private bool _show = false;
        private ObjectIconType _type = ObjectIconType.Neutral;

        private TickTimer _timer = null;

        public OverTimeTrait(TraitData data) : base(data)
        {
            if (data is OverTimeTraitData timeData)
            {
                _timerTicks = timeData.TicksToComplete;
                _loops = timeData.Loops;
                _applyOnTimerFinished = timeData.ApplyOnLoopComplete;
                _applyOnStart = timeData.ApplyOnStart;
                _status = timeData.Status;
                _damageInterruptable = timeData.DamageInterruptable;
                _show = timeData.Display;
                _type = timeData.IconType;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            _timer = new TickTimer(_timerTicks, TimerCompleted, _loops, LoopsCompleted);
            if (_applyOnStart)
            {
                TimerCompleted();
            }

            if (!string.IsNullOrWhiteSpace(_status))
            {
                WorldObject parentObj = null;
                this.SendMessageTo(new QueryWorldObjectMessage{DoAfter = obj => parentObj = obj}, _sender);
                _parent.Tile.EventsOnTile.Add(new CustomStatusWorldEvent{Status = _status, TargetId = _parent.Id, OwnerId = parentObj?.Id ?? string.Empty});
            }
            SubscribeToMessages();
        }

        private void TimerCompleted()
        {
            var traits = _applyOnTimerFinished.Select(TraitService.GetTrait).Where(t => t != null).ToArray();
            if (traits.Length > 0)
            {
                var addTraitToObjMsg = new AddTraitToObjectMessage();
                for (var i = 0; i < traits.Length; i++)
                {
                    addTraitToObjMsg.Trait = traits[i];
                    this.SendMessageTo(addTraitToObjMsg, _parent);
                }
            }
        }

        private void LoopsCompleted()
        {
            _timer.Destroy();
            _timer = null;
            this.SendMessageTo(new RemoveTraitFromObjectMessage{Trait = this}, _parent);
        }

        private void SubscribeToMessages()
        {
            _parent.SubscribeWithFilter<RefreshTimerMessage>(RefreshTimer, Name);
            if (_show)
            {
                _parent.SubscribeWithFilter<QueryClientIconDataMessage>(QueryClientObjectIcon, _instanceId);
            }
            if (_damageInterruptable)
            {
                _parent.SubscribeWithFilter<TakeDamageMessage>(TakeDamage, _instanceId);
            }
        }

        private void RefreshTimer(RefreshTimerMessage msg)
        {
            _timer?.Reset();
        }

        private void TakeDamage(TakeDamageMessage msg)
        {
            this.SendMessageTo(new RemoveTraitFromObjectMessage{Trait = this}, _parent);
        }

        private void QueryClientObjectIcon(QueryClientIconDataMessage msg)
        {
            if (_timer != null)
            {
                var timeLeft = _timer.TickCount;
                if (_timer.LoopCount < _loops - 1)
                {
                    timeLeft += _timerTicks * (_loops - _timer.LoopCount - 1);
                }

                var timerTicks = _timerTicks;
                if (_loops > 0)
                {
                    timerTicks = _timerTicks * _loops;
                }
                msg.DoAfter.Invoke(new ClientObjectIconData{Icon = Name, Id = _instanceId, MaxTicks = timerTicks, Ticks = timeLeft, Type = _type});
            }
        }

        public override void Destroy()
        {
            if (_timer != null)
            {
                _timer.Destroy();
                _timer = null;
            }

            _applyOnTimerFinished = null;
            base.Destroy();

        }
    }
}