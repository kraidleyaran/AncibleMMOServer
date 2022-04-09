using System;
using AncibleCoreCommon.CommonData.Ability;

namespace AncibleCoreCommon.CommonData.Client
{
    [Serializable]
    public class ClientResourceData
    {
        public ResourceType Resource;
        public int Current;
        public int Maximum;
        public int Bonus;
    }
}