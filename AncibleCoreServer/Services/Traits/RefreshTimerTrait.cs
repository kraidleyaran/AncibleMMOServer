using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class RefreshTimerTrait : ObjectTrait
    {
        public override bool Instant => true;

        private string _timer = string.Empty;

        public RefreshTimerTrait(TraitData data) : base(data)
        {
            if (data is RefreshTimerTraitData timerData)
            {
                _timer = timerData.Timer;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            this.SendMessageTo(new RefreshTimerMessage{Timer = _timer} , _parent);
        }
    }
}