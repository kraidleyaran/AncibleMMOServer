using System;

namespace AncibleCoreServer
{
    [Serializable]
    public class ServerSettings
    {
        public int Port { get; set; }
        public int TimeBetweenMessageChecks { get; set; }
        public string MapPath { get; set; }
        public string TraitFolder { get; set; }
        public string StartupScript { get; set; }
        public string ObjectSpawnPath { get; set; }
        public string CharacterClassPath { get; set; }
        public string AbilityPath { get; set; }
        public string TalentPath { get; set; }
        public string ObjectTemplatePath { get; set; }
        public string ItemPath { get; set; }
        public string LootTablePath { get; set; }
        public string CombatSettingsPath { get; set; }
        public string DatabaseSettingsPath { get; set; }
        public string AnalyticsDatabasePath { get; set; }
    }
}