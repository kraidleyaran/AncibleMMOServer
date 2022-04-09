using System;
using MessageBusLib;

namespace AncibleCoreServer
{
    public class WorldService
    {
        public virtual string Name => "Default World Service";

        public virtual void Start()
        {
            Log($"{Name} started");
        }

        public virtual void Stop()
        {
            this.UnsubscribeFromAllMessages();
            Log($"{Name} stopped");
        }

        public virtual void Log(string message)
        {
            Console.WriteLine($"{Name}-{DateTime.Now:HH:mm:ss tt} - {message}");
        }
    }

}