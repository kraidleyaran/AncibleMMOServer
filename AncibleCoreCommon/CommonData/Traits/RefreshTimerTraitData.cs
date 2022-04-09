using System;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class RefreshTimerTraitData : TraitData
    {
        public const string TYPE = "Refresh Timer Trait";
        public override string Type => TYPE;

        public string Timer;

    }
}