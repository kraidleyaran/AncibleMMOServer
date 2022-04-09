using Newtonsoft.Json;

namespace AncibleCoreServer.Services.Command
{
    [JsonObject(MemberSerialization.Fields)]
    public class ScriptCommand
    {
        public string Command;
        public string[] Arguments;
    }
}