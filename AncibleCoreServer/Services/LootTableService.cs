using System.Collections.Generic;
using System.IO;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Items;
using FileDataLib;
using MessageBusLib;

namespace AncibleCoreServer.Services
{
    public class LootTableService : WorldService
    {
        public static int ChestTicks { get; private set; }

        private static LootTableService _instance = null;

        public override string Name => "Loot Table Service";

        private Dictionary<string, LootTableData> _lootTables = new Dictionary<string, LootTableData>();

        private string _lootTablePath = string.Empty;

        public LootTableService(string lootTablePath)
        {
            _lootTablePath = lootTablePath;
        }

        public override void Start()
        {
            if (_instance == null)
            {
                _instance = this;
                var fileCount = 0;
                if (Directory.Exists(_lootTablePath))
                {
                    var files = Directory.GetFiles(_lootTablePath, $"*.{DataExtensions.LOOT_TABLE}");
                    fileCount = files.Length;
                    for (var i = 0; i < files.Length; i++)
                    {
                        var response = FileData.LoadData<LootTableData>(files[i]);
                        if (response.Success)
                        {
                            if (!_lootTables.ContainsKey(response.Data.Name))
                            {
                                _lootTables.Add(response.Data.Name, response.Data);
                            }
                        }
                        else
                        {
                            Log(response.HasException ? $"Exception while loading Loot Table at {files[i]} - {response.Exception}" : $"Unknown error while loading Loot Table at {files[i]}");
                        }
                    }
                }

                Log($"Loaded {_lootTables.Count} out of {fileCount} Loot Tables");
                SubscribeToMessages();
                base.Start();
            }
            
        }

        public static LootTableData GetLootTable(string name)
        {
            if (_instance._lootTables.TryGetValue(name, out var lootTable))
            {
                return lootTable;
            }

            return null;
        }

        private void SubscribeToMessages()
        {
            this.Subscribe<SetChestTicksMessage>(SetChestTicks);
        }

        private void SetChestTicks(SetChestTicksMessage msg)
        {
            ChestTicks = msg.Ticks;
            Log($"Chest ticks set to {ChestTicks}");
        }
    }
}