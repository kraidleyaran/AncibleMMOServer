using System.Collections.Generic;
using System.Linq;
using MessageBusLib;

namespace AncibleCoreServer.Services.Chat
{
    public class ChatService : WorldService
    {
        public static string[] DefaultChannels { get; private set; }

        private static ChatService _instance = null;

        public override string Name => "Chat Service";

        private Dictionary<string, ChatChannel> _channels = new Dictionary<string, ChatChannel>();

        public override void Start()
        {
            if (_instance == null)
            {
                _instance = this;
                SubscribeToMessages();
                base.Start();
            }
        }

        public static void RegisterUserToChannel(int networkId, string channel)
        {
            if (_instance._channels.TryGetValue(channel, out var chatChannel))
            {
                chatChannel.RegisterClient(networkId);
            }
        }

        public static void UnregisterUserFromChannel(int networkId, string channel)
        {
            if (_instance._channels.TryGetValue(channel, out var chatChannel))
            {
                chatChannel.UnregisterClient(networkId);
            }
        }

        public static void SendChatToChannel(string message, string owner, string ownerId, string channel)
        {
            if (_instance._channels.TryGetValue(channel, out var chatChannel))
            {
                chatChannel.AddMessage(message, owner, ownerId);
            }
        }

        private void SubscribeToMessages()
        {
            this.Subscribe<SetDefaultChatChannelsMessage>(SetDefaultChatChannels);
        }

        private void SetDefaultChatChannels(SetDefaultChatChannelsMessage msg)
        {
            var channels = msg.Channels;
            for (var i = 0; i < channels.Length; i++)
            {
                if (!_channels.ContainsKey(channels[i]))
                {
                    _channels.Add(channels[i], new ChatChannel(channels[i]));
                }
            }

            var channelNames = _channels.Keys.ToArray();
            var message = "Default Chat Channels Set -";
            for (var i = 0; i < channelNames.Length; i++)
            {
                message = i == 0 ? $"{message} {channels[i]}" : $"{message}, {channels[i]}";
            }

            DefaultChannels = channels.ToArray();
            Log(message);
        }
    }
}