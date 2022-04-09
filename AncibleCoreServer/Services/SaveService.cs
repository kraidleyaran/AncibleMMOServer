using System;
using MessageBusLib;

namespace AncibleCoreServer.Services
{
    public class SaveService : WorldService
    {
        public override string Name => "Save Service";

        private int _saveTicks = 0;
        private int _currentTicks = 0;

        public SaveService(int saveTicks)
        {
            _saveTicks = saveTicks;
        }

        public override void Start()
        {
            base.Start();
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            this.Subscribe<ResolveTickMessage>(ResolveTick);
            this.Subscribe<WorldSaveMessage>(WorldSave);
        }

        private void ResolveTick(ResolveTickMessage msg)
        {
            _currentTicks++;
            if (_currentTicks >= _saveTicks)
            {
                _currentTicks = 0;
                var startSave = DateTime.Now;
                Log($"Begin Save - {startSave:G}");
                this.SendMessage(SaveDataMessage.INSTANCE);
                var endSave = DateTime.Now;
                Log($"End Save - {endSave:G} - {endSave - startSave:g}");
            }
        }

        private void WorldSave(WorldSaveMessage msg)
        {
            _currentTicks = 0;
            var startSave = DateTime.Now;
            Log($"Begin Save - {startSave:G}");
            this.SendMessage(SaveDataMessage.INSTANCE);
            var endSave = DateTime.Now;
            Log($"End Save - {endSave:G} - {endSave - startSave:g}");
        }
    }
}