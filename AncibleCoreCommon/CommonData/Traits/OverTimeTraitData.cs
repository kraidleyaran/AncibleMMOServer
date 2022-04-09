using System;
using AncibleCoreCommon.CommonData.Client;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class OverTimeTraitData : TraitData
    {
        public const string TYPE = "Over Time Trait";
        public override string Type => TYPE;

        public int TicksToComplete;
        public int Loops;
        public string[] ApplyOnLoopComplete;
        public bool ApplyOnStart;
        public string Status;
        public bool DamageInterruptable;
        public bool Display;
        public ObjectIconType IconType;
    }
}