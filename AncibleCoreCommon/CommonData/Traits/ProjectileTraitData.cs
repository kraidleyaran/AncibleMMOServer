using System;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class ProjectileTraitData : TraitData
    {
        public const string TYPE = "Projectile Trait";
        public override string Type => TYPE;

        public int TravelTime;
        public string[] ApplyOnContact;
        public string Projectile;
    }
}