using System;

namespace AncibleCoreCommon.CommonData.Traits
{
    [Serializable]
    public class ObjectSpawnerTraitData : TraitData
    {
        public const string TYPE = "Object Spawner Trait";
        public override string Type => TYPE;

        public string Template;
        public int SpawnDistance;
        public int MaxSpawns;
        public int SpawnCooldown;
    }
}