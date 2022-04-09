using System.Collections.Generic;
using System.IO;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Items;
using FileDataLib;
using MessageBusLib;

namespace AncibleCoreServer.Services.Items
{
    public class ItemService : WorldService
    {
        public static int StartingMaxInventorySlots { get; private set; }
        private static ItemService _instance = null;

        public override string Name => "Item Service";

        private string _itemPath = string.Empty;

        private Dictionary<string, ItemData> _items = new Dictionary<string, ItemData>();

        public ItemService(string itemPath)
        {
            _itemPath = itemPath;
        }

        public override void Start()
        {
            if (_instance == null)
            {
                _instance = this;
                var fileCount = 0;
                if (Directory.Exists(_itemPath))
                {
                    var files = Directory.GetFiles(_itemPath, $"*.{DataExtensions.ITEM}");
                    fileCount = files.Length;
                    for (var i = 0; i < files.Length; i++)
                    {
                        var response = FileData.LoadData<ItemData>(files[i]);
                        if (response.Success)
                        {
                            if (!_items.ContainsKey(response.Data.Name))
                            {
                                _items.Add(response.Data.Name, response.Data);
                            }
                        }
                        else
                        {
                            Log(response.HasException ? $"Exception while loading Item at {files[i]} - {response.Exception}" : $"Unknown error while loading Item at {files[i]}");
                        }
                    }
                }
                Log($"Loaded {_items.Count} out of {fileCount} Items");
                SubscribeToMessages();
                base.Start();
            }
        }

        public static ItemData GetItemByName(string name)
        {
            if (_instance._items.TryGetValue(name, out var item))
            {
                return item;
            }

            return null;
        }

        private void SubscribeToMessages()
        {
            this.Subscribe<SetStartingMaxInventorySlotsMessage>(SetStartingMaxInventorySlots);
        }

        private void SetStartingMaxInventorySlots(SetStartingMaxInventorySlotsMessage msg)
        {
            StartingMaxInventorySlots = msg.Max;
            Log($"Starting Max Inventory Slots set to {StartingMaxInventorySlots}");
        }
    }
}