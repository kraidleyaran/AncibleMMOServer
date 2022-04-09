using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class WeaponDamageTrait : ObjectTrait
    {
        private IntNumberRange _damage = new IntNumberRange{Minimum = 1, Maximum = 2};

        public WeaponDamageTrait(TraitData data) : base(data)
        {
            if (data is WeaponDamageTraitData weaponData)
            {
                _damage = weaponData.Damage;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _parent.SubscribeWithFilter<QueryWeaponDamageMessage>(QueryWeaponDamage, _instanceId);
        }

        private void QueryWeaponDamage(QueryWeaponDamageMessage msg)
        {
            msg.DoAfter.Invoke(_damage);
        }
    }
}