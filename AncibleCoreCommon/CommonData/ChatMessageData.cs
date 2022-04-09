using System;

namespace AncibleCoreCommon.CommonData
{
    [Serializable]
    public class ChatMessageData
    {
        public string Owner;
        public string OwnerId;
        public string Message;
        public string Channel;
    }
}