using System;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class ObjectTrait : IDisposable
    {
        public string Name;
        public int MaxStack = 1;
        
        public virtual bool Instant => false;
        

        protected internal WorldObject _parent;
        protected internal object _sender = null;

        protected internal string _instanceId;

        public ObjectTrait(TraitData data)
        {
            Name = data.Name;
            MaxStack = data.MaxStack;
            _instanceId = Guid.NewGuid().ToString();
        }

        public ObjectTrait()
        {
            _instanceId = Guid.NewGuid().ToString();
        }

        public void SetSender(object sender)
        {
            _sender = sender;
        }

        public virtual void Setup(WorldObject owner)
        {
            _parent = owner;
        }

        public virtual void ApplyRemoval(WorldObject owner)
        {

        }

        public virtual void Destroy()
        {
            if (Instant)
            {
                _parent = null;
                _sender = null;
            }
            else
            {
                this.UnsubscribeFromAllMessages();
                _parent.UnsubscribeFromAllMessagesWithFilter(_instanceId);
                _parent = null;
                _sender = null;
                _instanceId = string.Empty;
            }

        }

        public virtual void Dispose()
        {
            Name = string.Empty;
            MaxStack = 0;
            _parent = null;
            _sender = null;
            _instanceId = string.Empty;
        }
    }
}