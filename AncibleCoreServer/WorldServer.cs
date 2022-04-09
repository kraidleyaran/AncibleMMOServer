using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AncibleCoreCommon;
using AncibleCoreServer.Services;
using AncibleCoreServer.Services.Ability;
using AncibleCoreServer.Services.CharacterClass;
using AncibleCoreServer.Services.ClientManager;
using AncibleCoreServer.Services.Combat;
using AncibleCoreServer.Services.Command;
using AncibleCoreServer.Services.Database;
using AncibleCoreServer.Services.Items;
using AncibleCoreServer.Services.Maps;
using AncibleCoreServer.Services.ObjectManager;
using AncibleCoreServer.Services.Talents;
using AncibleCoreServer.Services.Traits;
using AncibleCoreServer.Services.WorldBonuses;
using MessageBusLib;
using Newtonsoft.Json;
using Telepathy;

namespace AncibleCoreServer
{
    public class WorldServer
    {
        private static WorldServer _instance = null;
        private const string SERVER_NAME = "World Server";

        public ServerState State { get; private set; }

        private Server _telepathyServer = null;

        private List<WorldService> _services = null;

        private List<ClientMessage> _clientMessages = new List<ClientMessage>();
        private Thread _clientMessagingThread = null;

        public static void SendMessageToClient(ClientMessage msg, int networkId)
        {
            msg.Sender = null;
            _instance._telepathyServer.Send(networkId, msg.ConvertToJson());
        }

        public static void SendMessageToAllClients(ClientMessage msg)
        {
            var clients = ClientManagerService.Clients.ToArray();
            for (var i = 0; i < clients.Length; i++)
            {
                SendMessageToClient(msg, clients[i].NetworkId);
            }
        }

        public static void DisconnectClient(int networkId)
        {
            _instance._telepathyServer.Disconnect(networkId);
        }

        public void Start(ServerSettings settings)
        {
            if (_instance == null)
            {
                Log($"Starting server on port {settings.Port}");
                _instance = this;
                _telepathyServer = new Server();
                Log("Connection Established");
                _telepathyServer.MaxMessageSize = 1000000;
                Log($"Max Message Size: {_telepathyServer.MaxMessageSize}");
                State = ServerState.Connected;
                
                var refreshRate = settings.TimeBetweenMessageChecks;
                //_telepathyServer.SendTimeout = refreshRate * 2;
                _telepathyServer.Start(settings.Port);
                _clientMessagingThread = new Thread(() =>
                {
                    Log("Messenger thread started");
                    while (State == ServerState.Connected)
                    {
                        ReadMessages();
                        Thread.Sleep(refreshRate);
                    }

                    _clientMessagingThread = null;
                });
                _clientMessagingThread.Start();
                Log("Starting services...");
                _services = new List<WorldService>
                {
                    new DatabaseService(settings.DatabaseSettingsPath),
                    new RNGService(),
                    new ClientManagerService(),
                    new CommandService(),
                    new AbilityService(settings.AbilityPath),
                    new TalentService(settings.TalentPath),
                    new ItemService(settings.ItemPath),
                    new LootTableService(settings.LootTablePath),
                    new TraitService(settings.TraitFolder),
                    new CombatService(settings.CombatSettingsPath),
                    new MapService(settings.MapPath),
                    new CharacterClassService(settings.CharacterClassPath),
                    new ObjectTemplateService(settings.ObjectTemplatePath),
                    new ObjectManagerService(settings.ObjectSpawnPath),
                    new WorldBonusService(settings.WorldBonusesPath),
                    new SaveService(3000),
                    new KeyAuthorityService(),
                    new TickService(50)
                };
                for (var i = 0; i < _services.Count; i++)
                {
                    _services[i].Start();
                }
                Log("Services started");
                Log("Server startup completed");
                if (File.Exists(settings.StartupScript))
                {
                    var fileText = File.ReadAllText(settings.StartupScript);
                    var startupScript = JsonConvert.DeserializeObject<ServerScript>(fileText);
                    if (startupScript != null)
                    {
                        Log("Running startup script");
                        startupScript.Execute();
                    }
                }
                SubscribeToMessages();
            }
        }

        public void Stop()
        {
            if (_instance == this)
            {
                Log("Server shutting down");
                _instance = null;
                _telepathyServer.Stop();
                State = ServerState.Disconnected;
                for (var i = 0; i < _services.Count; i++)
                {
                    _services[i].Stop();
                }
                
                
            }
        }

        private static void Log(string message)
        {
            var loggedMessage = $"{SERVER_NAME}-{DateTime.Now:HH:mm:ss tt} - {message}";
            Console.WriteLine(loggedMessage);
        }

        private void ReadMessages()
        {
            while (_telepathyServer.GetNextMessage(out var message))
            {
                switch (message.eventType)
                {
                    case EventType.Connected:
                        var registerClientMsg = new RegisterClientMessage { NetworkId = message.connectionId };
                        this.SendMessage(registerClientMsg);
                        break;
                    case EventType.Data:
                        ClientMessage clientMsg = null;
                        try
                        {
                            clientMsg = message.data.GenerateMessageFromJson();
                        }
                        catch (Exception ex)
                        {
                            Log($"Exception while reading message - {ex}");
                            DisconnectClient(message.connectionId);
                        }
                        var existingClient = ClientManagerService.Clients.Find(c => c.NetworkId == message.connectionId);
                        if (clientMsg != null)
                        {
                            if (existingClient != null && existingClient.WorldId == clientMsg.ClientId)
                            {

                                if (string.IsNullOrEmpty(clientMsg.Filter))
                                {
                                    lock (_clientMessages)
                                    {
                                        _clientMessages.Add(clientMsg);
                                    }
                                }
                                else if (clientMsg.Filter == existingClient.WorldId)
                                {
                                    lock (_clientMessages)
                                    {
                                        _clientMessages.Add(clientMsg);
                                    }
                                }
                            }
                        }
                        else
                        {
                            existingClient?.Disconnect();
                        }
                        break;
                    case EventType.Disconnected:
                        var unregisterClientMsg = new UnregisterClientMessage { NetworkId = message.connectionId };
                        this.SendMessage(unregisterClientMsg);
                        break;
                }
            }
        }

        private void SubscribeToMessages()
        {
            this.Subscribe<ProcessClientInputMessage>(ProcessClientInput);
        }

        private void ProcessClientInput(ProcessClientInputMessage msg)
        {
            ClientMessage[] messages = new ClientMessage[0];
            lock (_clientMessages)
            {
                messages = _clientMessages.ToArray();
                _clientMessages.Clear();
            }
            for (var i = 0; i < messages.Length; i++)
            {
                if (string.IsNullOrEmpty(messages[i].Filter))
                {
                    this.SendMessage(messages[i]);
                }
                else
                {
                    this.SendMessageWithFilter(messages[i], messages[i].Filter);
                }
            }
        }


    }
}
