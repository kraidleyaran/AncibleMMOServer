using System.Collections.Generic;
using System.Linq;
using AncibleCoreCommon;
using AncibleCoreServer.Services.Chat;
using AncibleCoreServer.Services.ClientManager;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class PlayerChatTrait : ObjectTrait
    {
        private const string NAME = "Player Chat Trait";

        private List<string> _joinedChannels = new List<string>();

        private int _networkId = -1;
        private string _worldId = string.Empty;

        public PlayerChatTrait(string worldId)
        {
            Name = NAME;
            _worldId = worldId;
            _networkId = ClientManagerService.GetClientConnectionId(worldId);
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            _joinedChannels = ChatService.DefaultChannels.ToList();
            for (var i = 0; i < _joinedChannels.Count; i++)
            {
                ChatService.RegisterUserToChannel(_networkId, _joinedChannels[i]);
            }
            SubscribeToMessages();
            WorldServer.SendMessageToClient(new ClientJoinedChannelsMessage{Channels = _joinedChannels.ToArray()}, _networkId);
        }

        private void SubscribeToMessages()
        {
            this.SubscribeWithFilter<ClientChatMessage>(ClientChat, _worldId);
        }

        private void ClientChat(ClientChatMessage msg)
        {
            if (_joinedChannels.Contains(msg.Channel))
            {
                ChatService.SendChatToChannel(msg.Message, _parent.DisplayName, _parent.Id, msg.Channel);
            }
        }

        public override void Destroy()
        {
            for (var i = 0; i < _joinedChannels.Count; i++)
            {
                ChatService.UnregisterUserFromChannel(_networkId, _joinedChannels[i]);
            }
            base.Destroy();
        }
    }
}