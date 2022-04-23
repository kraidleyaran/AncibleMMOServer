using System;
using System.Collections.Generic;
using System.Linq;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Client;
using AncibleCoreServer.Services.Maps;
using AncibleCoreServer.Services.Traits;
using MessageBusLib;

namespace AncibleCoreServer.Services.ObjectManager
{
    public class WorldObject
    {
        public string DisplayName;
        public string Subtitle;
        public string Id { get; private set; }
        public bool BeingDestroy { get; private set; }
        public bool Update;
        public string Map;
        public MapTile Tile;
        public string Sprite;
        public bool Visible = true;
        public bool ShowName = true;
        public List<InteractionType> Interactions = new List<InteractionType>();
        public List<string> VisibleList = new List<string>();

        private List<ObjectTrait> _traits = new List<ObjectTrait>();
        private ClientObjectData _data = new ClientObjectData();

        private QueryClientObjectDataMessage _queryClientObjectDataMsg = new QueryClientObjectDataMessage();

        public WorldObject()
        {
            Id = Guid.NewGuid().ToString();
            _data.ObjectId = Id;
            _data.Name = DisplayName;
            SubscribeToMessages();
        }

        public ClientObjectData GetClientObjectData()
        {
            _data.Name = DisplayName;
            _data.Subtitle = Subtitle;
            _data.Sprite = Sprite;
            _data.Position = Tile.Position;
            _data.Interactions = Interactions.ToArray();
            _data.ShowName = ShowName;
            _queryClientObjectDataMsg.Data = _data;
            this.SendMessageTo(_queryClientObjectDataMsg, this);
            var statusEffects = new List<ClientStatusEffectData>();
            this.SendMessageTo(new QueryClientStatusEffectsMessage{DoAfter = statusEffect =>
            {
                statusEffects.Add(statusEffect);
            }}, this);
            var icons = new List<ClientObjectIconData>();
            this.SendMessageTo(new QueryClientIconDataMessage{DoAfter = iconData => icons.Add(iconData)}, this);
            _data.Icons = icons.ToArray();
            _data.StatusEffects = statusEffects.ToArray();
            return _data;
        }

        private void SubscribeToMessages()
        {
            this.Subscribe<AddTraitToObjectMessage>(AddTraitToObject);
            this.Subscribe<RemoveTraitFromObjectMessage>(RemoveTraitFromObject);
            this.SubscribeWithFilter<QueryWorldObjectMessage>(QueryWorldObject, Id);
        }

        private void AddTraitToObject(AddTraitToObjectMessage msg)
        {
            if (msg.Trait.Instant)
            {
                msg.Trait.SetSender(msg.Sender);
                msg.Trait.Setup(this);
                msg.Trait.Destroy();
            }
            else
            {
                var traitCount = _traits.Count(t => t.Name == msg.Trait.Name);
                if (msg.Trait.MaxStack <= 0 || traitCount < msg.Trait.MaxStack)
                {
                    _traits.Add(msg.Trait);
                    msg.Trait.SetSender(msg.Sender);
                    msg.Trait.Setup(this);
                }
            }
        }

        private void RemoveTraitFromObject(RemoveTraitFromObjectMessage msg)
        {
            if (_traits.Remove(msg.Trait))
            {
                msg.Trait.Destroy();
            }
        }

        private void QueryWorldObject(QueryWorldObjectMessage msg)
        {
            msg.DoAfter.Invoke(this);
        }

        public void Destroy()
        {
            Id = string.Empty;
            Sprite = string.Empty;
            Tile = null;
            BeingDestroy = true;
            for (var i = 0; i < _traits.Count; i++)
            {
                _traits[i].Destroy();
            }
            _traits.Clear();
            this.UnsubscribeFromAllMessages();
        }
    }
}