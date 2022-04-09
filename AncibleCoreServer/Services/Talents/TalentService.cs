using System.Collections.Generic;
using System.IO;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Talents;
using FileDataLib;

namespace AncibleCoreServer.Services.Talents
{
    public class TalentService : WorldService
    {
        private static TalentService _instance = null;

        public override string Name => "Talent Service";

        private string _talentFolderPath = string.Empty;

        private Dictionary<string, TalentData> _talents = new Dictionary<string, TalentData>();

        public TalentService(string talentFolderPath)
        {
            _talentFolderPath = talentFolderPath;
        }

        public override void Start()
        {
            if (_instance == null)
            {
                _instance = this;
                var fileCount = 0;
                if (Directory.Exists(_talentFolderPath))
                {
                    var files = Directory.GetFiles(_talentFolderPath, $"*.{DataExtensions.TALENT}");
                    fileCount = files.Length;
                    for (var i = 0; i < files.Length; i++)
                    {
                        var response = FileData.LoadData<TalentData>(files[i]);
                        if (response.Success)
                        {
                            if (!_talents.ContainsKey(response.Data.Name))
                            {
                                _talents.Add(response.Data.Name, response.Data);
                            }
                        }
                    }
                }

                Log($"Loaded {_talents.Count} out of {fileCount} Talents");
                base.Start();
            }
            

        }

        public static TalentData GetTalentByName(string name)
        {
            if (_instance._talents.TryGetValue(name, out var talent))
            {
                return talent;
            }

            return null;
        }
    }
}