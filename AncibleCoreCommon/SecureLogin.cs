using System;
using Newtonsoft.Json;

namespace AncibleCoreCommon
{
    [Serializable]
    [JsonObject(MemberSerialization.OptOut)]
    public class SecureLogin
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}