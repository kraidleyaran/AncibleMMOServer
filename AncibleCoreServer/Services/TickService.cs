using System;
using System.Threading;
using AncibleCoreCommon;
using MessageBusLib;

namespace AncibleCoreServer.Services
{
    public class TickService : WorldService, IDisposable
    {
        public override string Name => "Tick Service";
        public static int TickRate => _instance._timeBetweenTicks;

        private static TickService _instance = null;

        private int _timeBetweenTicks = 250;
        private Thread _tickThread = null;
        private bool _active = false;

        

        public TickService(int timeBetweenTicks)
        {
            _timeBetweenTicks = timeBetweenTicks;
        }

        public override void Start()
        {
            if (_instance == null)
            {
                _instance = this;
                _active = true;
                _tickThread = new Thread(() =>
                {
                    while (_active)
                    {
                        this.SendMessage(ProcessClientInputMessage.INSTANCE);
                        var clientWorldTickMsg = new ClientWorldTickMessage { Server = DateTime.UtcNow };
                        this.SendMessage(WorldTickMessage.INSTANCE);
                        this.SendMessage(UpdateClientsTickMessage.INSTANCE);
                        this.SendMessage(ResolveTickMessage.INSTANCE);
                        WorldServer.SendMessageToAllClients(clientWorldTickMsg);
                        Thread.Sleep(_timeBetweenTicks);   
                    }

                    _tickThread = null;
                });
                _tickThread.Start();
                base.Start();
            }


        }

        public override void Stop()
        {
            _active = false;
            base.Stop();
        }

        public void Dispose()
        {
            _active = false;
        }
    }
}