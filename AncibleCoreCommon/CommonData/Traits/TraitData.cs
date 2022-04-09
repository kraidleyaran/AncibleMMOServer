using System;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class TraitData
    {
        public const string DEFAULT_TYPE = "Default";

        public string Name;
        public virtual string Type => DEFAULT_TYPE;
        public int MaxStack = 1;
    }
}