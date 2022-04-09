using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AncibleCoreCommon;

namespace AncibleCoreServer.Services
{
    public class MessageAuthorityService : WorldService
    {
        public override string Name => "Message Authority Service";

        private Dictionary<string, Type> _messageTypes = new Dictionary<string, Type>();

        private static MessageAuthorityService _instance = null;

        public override void Start()
        {
            if (_instance == null)
            {
                _instance = this;
                var clientMessageTypes = Assembly.GetAssembly(typeof(ClientMessage)).GetTypes().Where(t => t.BaseType != null && t.BaseType.IsAssignableFrom(t)).ToArray();
                for (var i = 0; i < clientMessageTypes.Length; i++)
                {
                    if (!string.IsNullOrEmpty(clientMessageTypes[i].FullName))
                    {
                        _messageTypes.Add(clientMessageTypes[i].FullName, clientMessageTypes[i]);
                    }
                }
                Log($"{_messageTypes.Count} Messages Authorized");
                base.Start();
            }

        }

        public static bool IsValidMessage(string typeName)
        {
            return _instance._messageTypes.ContainsKey(typeName);
        }

        public static Type GetMessageType(string typeName)
        {
            if (_instance._messageTypes.TryGetValue(typeName, out var type))
            {
                return type;
            }

            return null;
        }
    }
}