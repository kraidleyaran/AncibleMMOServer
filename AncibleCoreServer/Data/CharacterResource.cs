using System;
using AncibleCoreCommon.CommonData.Ability;
using AncibleCoreCommon.CommonData.Client;

namespace AncibleCoreServer.Data
{
    [Serializable]
    public class CharacterResource : WorldData
    {
        public const string TABLE = "CharacterResource";

        public ResourceType Resource { get; set; }
        public int Current { get; set; }
        public int Maximum { get; set; }

        public CharacterResource()
        {

        }

        public CharacterResource(ClientResourceData data)
        {
            FromClientData(data);
        }

        public void FromClientData(ClientResourceData data)
        {
            Resource = data.Resource;
            Current = data.Current;
            Maximum = data.Maximum;
        }
    }
}