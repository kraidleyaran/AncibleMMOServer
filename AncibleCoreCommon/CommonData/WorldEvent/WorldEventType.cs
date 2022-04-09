using System;

namespace AncibleCoreCommon.CommonData.WorldEvent
{
    [Serializable]
    public enum WorldEventType
    {
        Default,Bump,Fx,Damage,Projectile,LevelUp,StatusEffect,CustomStatus,Heal,Resource,Cast,CancelCast
    }
}