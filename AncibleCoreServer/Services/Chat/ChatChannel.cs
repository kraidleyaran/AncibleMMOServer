using System.Collections.Generic;
using AncibleCoreCommon;
using AncibleCoreCommon.CommonData;
using AncibleCoreServer.Services.ClientManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Chat
{
    public class ChatChannel
    {
        public string Name { get; private set; }

        private List<ChatMessageData> _chatMessages = new List<ChatMessageData>();
        private List<int> _activeUserClientIds = new List<int>();

        public ChatChannel(string name)
        {
            Name = name;
            SubscribeToMessages();
        }

        public void RegisterClient(int id)
        {
            if (!_activeUserClientIds.Contains(id))
            {
                _activeUserClientIds.Add(id);
            }
        }

        public void UnregisterClient(int id)
        {
            _activeUserClientIds.Remove(id);
        }

        public void AddMessage(string message, string owner, string ownerId)
        {
            _chatMessages.Add(new ChatMessageData{Channel = Name, Owner = owner, OwnerId = ownerId, Message = message});
        }

        private void SubscribeToMessages()
        {
            this.Subscribe<UpdateClientsTickMessage>(UpdateClientsTick);
            this.Subscribe<ResolveTickMessage>(ResolveTick);
        }

        private void UpdateClientsTick(UpdateClientsTickMessage msg)
        {
            var messages = _chatMessages.ToArray();
            var clientChatUpdateMsg = new ClientChatUpdateMessage {Messages = messages};
            var activeUsers = _activeUserClientIds.ToArray();
            for (var i = 0; i < activeUsers.Length; i++)
            {
                WorldServer.SendMessageToClient(clientChatUpdateMsg, _activeUserClientIds[i]);
            }
        }

        private void ResolveTick(ResolveTickMessage msg)
        {
            _chatMessages.Clear();
        }

        public void Destroy()
        {
            _chatMessages.Clear();
            _chatMessages = null;
            this.UnsubscribeFromAllMessages();
        }

    }
}