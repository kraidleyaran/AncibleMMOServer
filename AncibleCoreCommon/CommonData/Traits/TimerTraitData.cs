using System;
using AncibleCoreCommon.CommonData.Client;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class TimerTraitData : TraitData
    {
        public const string TYPE = "Timer Trait";
        public override string Type => TYPE;

        public int TimerTicks;
        public string[] ApplyOnStart;
        public string[] ApplyOnEnd;
        public string Status;
        public bool DamageInterruptable;
        public bool Display;
        public ObjectIconType IconType;
    }
}