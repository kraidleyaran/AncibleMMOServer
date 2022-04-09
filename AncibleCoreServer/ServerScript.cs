using AncibleCoreServer.Services.Command;
using MessageBusLib;
using Newtonsoft.Json;

namespace AncibleCoreServer
{
    [JsonObject(MemberSerialization.Fields)]
    public class ServerScript
    {
        public ScriptCommand[] Commands;


        public void Execute()
        {
            for (var i = 0; i < Commands.Length; i++)
            {
                this.SendMessage(new ExecuteServerCommandMessage { Command = Commands[i].Command, Arguments = Commands[i].Arguments });
            }
        }
    }
}