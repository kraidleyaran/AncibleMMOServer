using AncibleCoreCommon;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Combat;
using AncibleCoreCommon.CommonData.Traits;
using AncibleCoreCommon.CommonData.WorldBonuses;
using AncibleCoreCommon.CommonData.WorldEvent;
using AncibleCoreServer.Services.Combat;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;

namespace AncibleCoreServer.Services.Traits
{
    public class HealTrait : ObjectTrait
    {
        public override bool Instant => true;

        private IntNumberRange _amount = new IntNumberRange{Minimum = 1, Maximum = 2};
        private DamageType _damageType = DamageType.Magical;
        private bool _applyBonus = false;
        private bool _broadcast = false;
        private string[] _tags = new string[0];

        public HealTrait(TraitData data) : base(data)
        {
            if (data is HealTraitData healData)
            {
                _amount = healData.Amount;
                _damageType = healData.DamageType;
                _applyBonus = healData.ApplyBonus;
                _broadcast = healData.Broadcast;
                _tags = healData.Tags;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            var amount = _amount.GenerateRandomNumber(RNGService.RANDOM);
            WorldObject parentObj = null;
            if (_sender != null)
            {
                this.SendMessageTo(new QueryWorldObjectMessage { DoAfter = obj => parentObj = obj }, _sender);
            }
            
            if (_applyBonus && parentObj != null)
            {
                var bonus = 0;
                this.SendMessageTo(new QueryCombatStatsMessage
                {
                    DoAfter =
                        (baseStats, bonusStats, currentHealth) =>
                        {
                            bonus += CombatService.CalculateHealBonus(baseStats + bonusStats, _damageType);
                        }
                }, parentObj);
                amount += bonus;
                var worldBonuses = new WorldBonusData[0];
                this.SendMessageTo(new QueryWorldBonusesByTagsMessage{Type = WorldBonusType.Heal, Tags = _tags, DoAfter = bonuses => worldBonuses = bonuses}, parentObj);
                amount += worldBonuses.GetBonusesTotal();
                if (_broadcast)
                {
                    this.SendMessageTo(new BroadcastHealMessage { Amount = amount, Owner = parentObj }, parentObj);
                }
            }
            _parent.Tile.EventsOnTile.Add(new HealWorldEvent{TargetId = _parent.Id, OwnerId = parentObj?.Id ?? string.Empty, Amount = amount});
            this.SendMessageTo(new HealMessage{Amount = amount, Owner = parentObj, Broadcast = _broadcast}, owner);

        }
    }
}