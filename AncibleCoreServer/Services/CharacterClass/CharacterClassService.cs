using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.CharacterClasses;
using AncibleCoreCommon.CommonData.Combat;
using AncibleCoreServer.Services.Traits;
using FileDataLib;
using MessageBusLib;

namespace AncibleCoreServer.Services.CharacterClass
{
    public class CharacterClassService : WorldService
    {


        public override string Name => "Character Class Service";

        public static float ExperienceMultiplier { get; private set; }
        public static int BaseExperience { get; private set; }
        public static int MaxLevel { get; private set; }

        private static CharacterClassService _instance = null;

        private string _characterClassPath = string.Empty;

        private Dictionary<string, CharacterClassData> _classes = new Dictionary<string, CharacterClassData>();

        private List<string> _startingClasses = new List<string>();

        public CharacterClassService(string characterClassPath)
        {
            _characterClassPath = characterClassPath;
        }

        public override void Start()
        {
            if (_instance == null)
            {
                _instance = this;
                var fileCount = 0;
                if (Directory.Exists(_characterClassPath))
                {
                    var files = Directory.GetFiles(_characterClassPath, $"*.{DataExtensions.CHARACTER_CLASS}");
                    fileCount = files.Length;
                    for (var i = 0; i < files.Length; i++)
                    {
                        var response = FileData.LoadData<CharacterClassData>(files[i]);
                        if (!_classes.ContainsKey(response.Data.Class))
                        {
                            _classes.Add(response.Data.Class, response.Data);
                        }
                    }
                }
                Log($"Loaded {_classes.Count} out of {fileCount} Character Classes");
                SubscribeToMessages();
                base.Start();
            }
            
        }

        public static bool DoesClassExist(string className)
        {
            return _instance._classes.ContainsKey(className);
        }

        public static bool IsStartingClass(string className)
        {
            return _instance._classes.ContainsKey(className);
        }

        public static string[] GetStartingClasses()
        {
            return _instance._startingClasses.ToArray();
        }

        public static CharacterClassData GetClassByName(string name)
        {
            if (_instance._classes.TryGetValue(name, out var classData))
            {
                return classData;
            }

            return null;
        }

        public static int GetLevelExperience(int level)
        {
            if (level < MaxLevel)
            {
                return (int)(BaseExperience + BaseExperience * (level * ExperienceMultiplier));
            }

            return -1;

        }

        public static CombatGrowthStats GetGrowthForClass(string name)
        {
            if (_instance._classes.TryGetValue(name, out var classData))
            {
                return classData.GrowthStats;
            }
            return new CombatGrowthStats();
        }

        public static ObjectTrait[] GetTraitsForClassLevel(string className, int level)
        {
            var levelIndex = level - 1;
            if (_instance._classes.TryGetValue(className, out var playerClass))
            {
                if (playerClass.LevelUpData.Length > levelIndex)
                {
                    return playerClass.LevelUpData[levelIndex].ApplyOnLevel.Select(TraitService.GetTrait).Where(t => t != null).ToArray();
                    
                }
            }
            return new ObjectTrait[0];
        }

        private void SubscribeToMessages()
        {
            this.Subscribe<SetStartingClassesMessage>(SetStartingClasses);
            this.Subscribe<SetClassExperienceSettingsMessage>(SetClassExperienceSettings);
            this.Subscribe<SetMaxLevelMessage>(SetMaxLevel);
        }

        private void SetStartingClasses(SetStartingClassesMessage msg)
        {
            var existingClasses = msg.Classes.Where(c => _classes.ContainsKey(c)).ToArray();
            _startingClasses = existingClasses.ToList();
            Log($"Starting Class count set to {_startingClasses.Count}");
        }

        private void SetClassExperienceSettings(SetClassExperienceSettingsMessage msg)
        {
            BaseExperience = msg.BaseExperience;
            ExperienceMultiplier = msg.ExperienceMultiplier;
        }

        private void SetMaxLevel(SetMaxLevelMessage msg)
        {
            MaxLevel = msg.Max - 1;
            Log($"Max Level set to {msg.Max}");
        }
    }
}