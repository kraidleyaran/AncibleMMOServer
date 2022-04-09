using System.Collections.Generic;
using System.IO;
using AncibleCoreCommon.CommonData;
using FileDataLib;

namespace AncibleCoreServer.Services
{
    public class ObjectTemplateService : WorldService
    {
        private static ObjectTemplateService _instance = null;

        public override string Name => "Object Template Service";

        private Dictionary<string, ObjectTemplateData> _objectTemplates = new Dictionary<string, ObjectTemplateData>();

        private string _templatePath = string.Empty;

        public ObjectTemplateService(string path)
        {
            _templatePath = path;
        }

        public override void Start()
        {
            if (_instance == null)
            {
                _instance = this;
                var fileCount = 0;
                if (Directory.Exists(_templatePath))
                {
                    var files = Directory.GetFiles(_templatePath, $"*.{DataExtensions.OBJECT_TEMPLATE}");
                    fileCount = files.Length;
                    for (var i = 0; i < files.Length; i++)
                    {
                        var response = FileData.LoadData<ObjectTemplateData>(files[i]);
                        if (response.Success)
                        {
                            if (!_objectTemplates.ContainsKey(response.Data.Name))
                            {
                                _objectTemplates.Add(response.Data.Name, response.Data);
                            }
                        }
                    }
                }
                Log($"Loaded {_objectTemplates.Count} of {fileCount} Object Templates");
                base.Start();
            }
            
        }

        public static ObjectTemplateData GetObjectTemplate(string templateName)
        {
            if (_instance._objectTemplates.TryGetValue(templateName, out var template))
            {
                return template;
            }

            return null;
        }
    }
}