using System.Collections.Generic;
using System.IO;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.WorldBonuses;
using FileDataLib;

namespace AncibleCoreServer.Services.WorldBonuses
{
    public class WorldBonusService : WorldService
    {
        private static WorldBonusService _instance = null;

        public override string Name => "World Bonus Service";

        private string _path = string.Empty;

        private Dictionary<string, WorldBonusData> _bonuses = new Dictionary<string, WorldBonusData>();

        public WorldBonusService(string path)
        {
            _path = path;
        }

        public override void Start()
        {
            if (_instance == null)
            {
                _instance = this;
                if (Directory.Exists(_path))
                {
                    var files = Directory.GetFiles(_path, $"*.{DataExtensions.WORLD_BONUS}");
                    for (var i = 0; i < files.Length; i++)
                    {
                        var response = FileData.LoadData<WorldBonusData>(files[i]);
                        if (response.Success)
                        {
                            if (!_bonuses.ContainsKey(response.Data.Name))
                            {
                                _bonuses.Add(response.Data.Name, response.Data);
                            }
                        }
                        else
                        {
                            Log(response.HasException ? $"Exception while loading World Bonus at path {files[i]} - {response.Exception}" : $"Unknown error while loading World Bonus at path {files[i]}");
                        }
                    }
                }
                base.Start();
            }
            
        }

        public static WorldBonusData GetBonusByName(string name)
        {
            if (_instance._bonuses.TryGetValue(name, out var worldBonus))
            {
                return worldBonus;
            }

            return null;
        }
    }
}