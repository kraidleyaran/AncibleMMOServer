using System.Collections.Generic;
using System.Linq;
using AncibleCoreCommon.CommonData.Ability;
using AncibleCoreCommon.CommonData.Client;

namespace AncibleCoreServer.Services.Ability
{
    public class Ability
    {
        public string Name;
        public int Rank;
        public int Cooldown;
        public bool OnCooldown => _cooldownTimer != null;
        public List<string> OwnerMods;
        public List<string> TargetMods;

        private TickTimer _cooldownTimer = null;

        public Ability(AbilityData data, string[] ownerMods, string[] targetMods, int rank = 0, int cooldown = 0)
        {
            Name = data.Name;
            Rank = rank;
            Cooldown = data.Cooldown;
            OwnerMods = ownerMods.ToList();
            TargetMods = targetMods.ToList();
            if (cooldown > 0)
            {
                var maxCooldown = cooldown;
                if (maxCooldown > data.Cooldown)
                {
                    maxCooldown = data.Cooldown;
                }
                _cooldownTimer = new TickTimer(maxCooldown, () =>
                {
                    _cooldownTimer.Destroy();
                    _cooldownTimer = null;
                });
            }
        }

        public void TriggerCooldown()
        {
            if (_cooldownTimer != null)
            {
                _cooldownTimer.Destroy();
                _cooldownTimer = null;
            }

            _cooldownTimer = new TickTimer(Cooldown, () =>
            {
                _cooldownTimer.Destroy();
                _cooldownTimer = null;
            });
        }

        public ClientAbilityInfoData GetClientData()
        {
            return new ClientAbilityInfoData { Name = Name, Cooldown = Cooldown, CurrentCooldownTicks = _cooldownTimer?.TickCount ?? 0, Rank = Rank, OwnerMods = OwnerMods.ToArray(), TargetMods = TargetMods.ToArray()};
        }


    }
}