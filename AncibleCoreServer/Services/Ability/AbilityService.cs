using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Ability;
using FileDataLib;
using MessageBusLib;

namespace AncibleCoreServer.Services.Ability
{
    public class AbilityService : WorldService
    {
        public static string[] StartingAbilities { get; private set; }
        public static int GlobalCooldown { get; private set; }

        private static AbilityService _instance = null;

        public override string Name => "Ability Service";

        private string _abilityPath = string.Empty;

        private Dictionary<string, AbilityData> _abilities = new Dictionary<string, AbilityData>();

        

        public AbilityService(string abilityPath)
        {
            _abilityPath = abilityPath;
        }

        public override void Start()
        {
            if (_instance == null)
            {
                _instance = this;
                var fileCount = 0;
                if (Directory.Exists(_abilityPath))
                {
                    var files = Directory.GetFiles(_abilityPath, $"*.{DataExtensions.ABILITY}");
                    fileCount = files.Length;
                    for (var i = 0; i < files.Length; i++)
                    {
                        var response = FileData.LoadData<AbilityData>(files[i]);
                        if (response.Success)
                        {
                            if (!_abilities.ContainsKey(response.Data.Name))
                            {
                                _abilities.Add(response.Data.Name, response.Data);
                            }
                        }
                        else
                        {
                            Log(response.HasException ? $"Exception while loading Ability at {files[i]} - {response.Exception}" : $"Unknown error while loading Ability at {files[i]}");
                        }
                    }
                }
                Log($"Loaded {_abilities.Count} out of {fileCount} Abilities");
                SubscribeToMessages();
                base.Start();
            }
            
        }

        public static AbilityData GetAbilityByName(string abilityName)
        {
            if (_instance._abilities.TryGetValue(abilityName, out var ability))
            {
                return ability;
            }

            return null;
        }

        public static AbilityData[] GetStartingAbilities()
        {
            var abilities = new List<AbilityData>();
            for (var i = 0; i < StartingAbilities.Length; i++)
            {
                var ability = GetAbilityByName(StartingAbilities[i]);
                if (ability != null)
                {
                    abilities.Add(ability);
                }
            }

            return abilities.ToArray();

        }

        public static bool DoesAbilityExist(string abilityName)
        {
            return _instance._abilities.ContainsKey(abilityName);
        }

        private void SubscribeToMessages()
        {
            this.Subscribe<SetStartingAbilitiesMessage>(SetStartingAbilities);
            this.Subscribe<SetGlobalCooldownMessage>(SetGlobalCooldown);
        }

        private void SetStartingAbilities(SetStartingAbilitiesMessage msg)
        {
            var starting = new List<string>();
            for (var i = 0; i < msg.Abilities.Length; i++)
            {
                if (DoesAbilityExist(msg.Abilities[i]))
                {
                    starting.Add(msg.Abilities[i]);
                }
            }

            StartingAbilities = starting.ToArray();
            Log($"Set {StartingAbilities.Length} Starting Abilities");
        }

        private void SetGlobalCooldown(SetGlobalCooldownMessage msg)
        {
            GlobalCooldown = msg.Ticks;
            Log($"Global Cooldown set to {GlobalCooldown} Ticks");
        }
    }
}