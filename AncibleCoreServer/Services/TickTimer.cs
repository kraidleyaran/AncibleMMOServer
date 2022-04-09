using System;
using MessageBusLib;

namespace AncibleCoreServer.Services
{
    public class TickTimer
    {
        public int TickCount { get; private set; }
        public int LoopCount => _loopCount;

        private Action _doAfter;
        private Action _onDestroy;
        private int _maxTicks;
        private int _loop = 0;
        private int _loopCount = 0;
        private bool _active = false;

        private bool _paused = false;

        public TickTimer(int ticks, Action doAfter, int loop = 0, Action onDestroy = null)
        {
            _maxTicks = ticks;
            TickCount = _maxTicks;
            _doAfter = doAfter;
            _onDestroy = onDestroy;
            _loop = loop;
            _active = true;
            SubscribeToMessages();
        }

        public void TogglePause()
        {
            _paused = !_paused;
        }


        private void SubscribeToMessages()
        {
            this.Subscribe<WorldTickMessage>(WorldTick);
        }

        private void WorldTick(WorldTickMessage msg)
        {
            if (_active && !_paused)
            {
                TickCount--;
                if (TickCount <= 0)
                {
                    _doAfter.Invoke();
                    if (_loop < 0)
                    {
                        TickCount = _maxTicks;
                    }
                    else
                    {
                        _loopCount++;
                        if (_loopCount <= _loop)
                        {
                            TickCount = _maxTicks;
                        }
                        else
                        {
                            _onDestroy?.Invoke();
                            _active = false;
                            this.Unsubscribe<WorldTickMessage>();
                        }
                    }
                }
            }

        }

        public void Reset()
        {
            TickCount = _maxTicks;
            _loopCount = 0;
            if (!_active)
            {
                _active = true;
                this.Subscribe<WorldTickMessage>(WorldTick);
            }
        }

        public void Destroy()
        {
            _active = false;
            _doAfter = null;
            _onDestroy = null;
            _maxTicks = 0;
            TickCount = 0;
            _loop = 0;
            _loopCount = 0;
            this.UnsubscribeFromAllMessages();
        }
    }
}