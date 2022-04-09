using System.Collections.Generic;
using System.Linq;
using AncibleCoreCommon.CommonData.WorldBonuses;
using AncibleCoreServer.Data;
using AncibleCoreServer.Services.ObjectManager;
using AncibleCoreServer.Services.WorldBonuses;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class PlayerBonusManagerTrait : ObjectTrait
    {
        private const string NAME = "Player Bonus ManagerTrait";

        private List<WorldBonusData> _bonuses = new List<WorldBonusData>();

        public PlayerBonusManagerTrait(CharacterWorldBonus[] bonuses)
        {
            Name = NAME;
            for (var i = 0; i < bonuses.Length; i++)
            {
                var bonus = WorldBonusService.GetBonusByName(bonuses[i].Bonus);
                if (bonus != null)
                {
                    _bonuses.Add(bonus);
                }
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            SubscribeToMesages();
        }

        private void SubscribeToMesages()
        {
            _parent.SubscribeWithFilter<AddWorldBonusMessage>(AddWorldBonus, _instanceId);
            _parent.SubscribeWithFilter<RemoveWorldBonusMessage>(RemoveWorldBonus, _instanceId);
            _parent.SubscribeWithFilter<QueryWorldBonusesMessage>(QueryWorldBonuses, _instanceId);
            _parent.SubscribeWithFilter<QueryWorldBonusesByTagsMessage>(QueryWorldBonusesByTag, _instanceId);
        }

        private void AddWorldBonus(AddWorldBonusMessage msg)
        {
            //TODO: Should multiple of the same world bonus be able to be added?
            _bonuses.Add(msg.Bonus);
        }

        private void RemoveWorldBonus(RemoveWorldBonusMessage msg)
        {
            _bonuses.Remove(msg.Bonus);
        }

        private void QueryWorldBonuses(QueryWorldBonusesMessage msg)
        {
            msg.DoAfter.Invoke(_bonuses.ToArray());
        }

        private void QueryWorldBonusesByTag(QueryWorldBonusesByTagsMessage msg)
        {
            msg.DoAfter.Invoke(_bonuses.Where(b => b.Type == msg.Type && b.Tags.Any(t => msg.Tags.Any(bt => bt == t))).ToArray());
        }
    }
}