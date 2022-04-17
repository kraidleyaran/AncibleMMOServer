using System;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class ApplyWorldBonusTraitData : TraitData
    {
        public const string TYPE = "Apply World Bonus Trait";
        public override string Type => TYPE;

        public string Bonus;
        public bool Permanent;
    }
}