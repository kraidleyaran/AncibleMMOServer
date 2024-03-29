﻿using System;
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
    public class DamageTrait : ObjectTrait
    {
        public override bool Instant => true;

        private IntNumberRange _amount = new IntNumberRange {Minimum = 1, Maximum = 2};
        private DamageType _type = DamageType.Physical;
        private float _bonusMultiplier = 1f;
        private bool _useWeaponDamage = false;
        private string[] _tags = new string[0];
        private bool _ignoreDodge = false;

        public DamageTrait(TraitData data) : base(data)
        {
            if (data is DamageTraitData damageData)
            {
                _amount = damageData.Amount;
                _type = damageData.DamageType;
                _bonusMultiplier = damageData.BonusMultiplier;
                _useWeaponDamage = damageData.UseWeaponDamage;
                _tags = damageData.Tags;
            }
        }

        public override void Setup(WorldObject owner)
        {
            base.Setup(owner);
            var dodged = false;
            if (!_ignoreDodge)
            {
                var parentStats = new CombatStats();
                this.SendMessageTo(new QueryCombatStatsMessage{DoAfter = (baseStats, bonusStats, currentHealth) =>
                    {
                        parentStats = baseStats + bonusStats;
                    }}, _parent);
                dodged = CombatService.CalculateDodage(parentStats);
            }

            if (dodged)
            {
                WorldObject damageOwner = null;
                var sender = _sender ?? this;
                this.SendMessageTo(new QueryWorldObjectMessage { DoAfter = obj => damageOwner = obj }, sender);
                _parent.Tile.EventsOnTile.Add(new DodgeWorldEvent{OriginId = _parent.Id, OwnerId = damageOwner != null ? damageOwner.Id : string.Empty});
            }
            else
            {
                WorldObject damageOwner = null;
                var sender = _sender ?? this;
                this.SendMessageTo(new QueryWorldObjectMessage { DoAfter = obj => damageOwner = obj }, sender);
                var amount = _amount.GenerateRandomNumber(RNGService.RANDOM);
                var crit = false;
                if (damageOwner != null)
                {
                    if (_useWeaponDamage)
                    {
                        var weaponDamage = new IntNumberRange { Minimum = 0, Maximum = 0 };
                        var weaponDamageEquipped = false;
                        this.SendMessageTo(new QueryWeaponDamageMessage
                        {
                            DoAfter = equippedDamage =>
                            {
                                weaponDamage += equippedDamage;
                                weaponDamageEquipped = true;
                            }
                        }, damageOwner);
                        if (weaponDamageEquipped)
                        {
                            amount += weaponDamage.GenerateRandomNumber(RNGService.RANDOM);
                        }
                    }

                    var ownerStats = new CombatStats();
                    this.SendMessageTo(new QueryCombatStatsMessage
                    {
                        DoAfter = (baseStats, bonusStats, currentHealth) =>
                        {
                            ownerStats = baseStats + bonusStats;
                        }
                    }, damageOwner);
                    amount += (int)(CombatService.CalculateBonusDamage(_type, ownerStats) * _bonusMultiplier);
                    var worldBonuses = new WorldBonusData[0];
                    this.SendMessageTo(new QueryWorldBonusesByTagsMessage { Tags = _tags, Type = WorldBonusType.Damage, DoAfter = bonuses => worldBonuses = bonuses }, damageOwner);
                    amount += worldBonuses.GetBonusesTotal(amount);
                    var critAmount = CombatService.CalculateCritDamage(_type, ownerStats, amount);
                    if (critAmount != amount)
                    {
                        amount = critAmount;
                        crit = true;
                    }
                }
                var targetStats = new CombatStats();
                this.SendMessageTo(new QueryCombatStatsMessage
                {
                    DoAfter = (baseStats, bonusStats, currentHealth) =>
                    {
                        targetStats = baseStats + bonusStats;
                    }
                }, _parent);
                var resisted = CombatService.CalculateResistedDamage(_type, targetStats, amount);
                amount -= resisted;
                var damageEvent = new DamageEvent
                {
                    Amount = amount,
                    DamageType = _type,
                    OriginId = damageOwner != null ? damageOwner.Id : string.Empty,
                    TargetId = owner.Id,
                    CriticalStrike = crit
                };
                if (damageOwner != null)
                {
                    this.SendMessageTo(new DamageReportMessage { Amount = amount, Type = _type }, damageOwner);
                }

                var originDisplayName = damageOwner == null ? string.Empty : damageOwner.DisplayName;
                damageEvent.Text = string.IsNullOrEmpty(damageEvent.OriginId) ? $"{owner.DisplayName} takes {amount} {_type} damage" : $"{originDisplayName} does {amount} {_type} damage to {owner.DisplayName}";
                owner.Tile.EventsOnTile.Add(damageEvent);
                sender.SendMessageTo(new TakeDamageMessage { Amount = amount, Owner = damageOwner, Type = _type }, owner);
            }

        }
    }
}