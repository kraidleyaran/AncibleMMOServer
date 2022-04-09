using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AncibleCoreServer;
using MessageBusLib;
using Newtonsoft.Json;

namespace AncibleCoreConsole
{
    class Program
    {
        private static ServerSettings _settings = new ServerSettings
        {
            Port = 42069,
            TimeBetweenMessageChecks = 10,
            MapPath = @"D:\CauldronMMO\Data\World Maps",
            TraitFolder = @"D:\CauldronMMO\Data\Traits",
            StartupScript = "D:\\CauldronMMO\\startup script.json",
            ObjectSpawnPath = @"D:\CauldronMMO\Data\Spawn Maps"

        };
        private static WorldServer _worldServer = null;
        private static bool _run = true;
        private static Thread _consoleInputThread = null;

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (File.Exists(args[0]))
                {
                    var json = File.ReadAllText(args[0]);
                    var serverSettings = JsonConvert.DeserializeObject<ServerSettings>(json);
                    if (serverSettings != null)
                    {
                        _settings = serverSettings;
                        _worldServer = new WorldServer();
                        _worldServer.Start(_settings);
                        _consoleInputThread = new Thread(() =>
                        {
                            while (_run)
                            {
                                var userInput = Console.ReadLine();
                                if (!string.IsNullOrEmpty(userInput))
                                {
                                    Console.WriteLine("=> Executing command");
                                    var inputWords = userInput.Split(' ').ToList();
                                    var command = inputWords[0];
                                    var arguments = new string[0];
                                    inputWords.RemoveAt(0);
                                    if (inputWords.Count > 0)
                                    {
                                        arguments = inputWords.ToArray();
                                    }
                                    ExecuteCommand(command, arguments);
                                    Thread.Sleep(10);
                                }
                            }
                        });
                        _consoleInputThread.Start();
                    }
                    else
                    {
                        Console.WriteLine("Invalid Server Settings");
                    }
                }
                else
                {
                    Console.WriteLine("Missing Server Settings Path argument");
                }

            }

        }

        private static void ExecuteCommand(string command, string[] args)
        {
            _worldServer.SendMessage(new ExecuteServerCommandMessage
            {
                Command = command,
                Arguments = args
            });
        }

        
    }
}
