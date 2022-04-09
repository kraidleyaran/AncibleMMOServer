using System.Collections.Generic;
using System.IO;
using System.Linq;
using AncibleCoreCommon.CommonData;
using AncibleCoreCommon.CommonData.Traits;
using FileDataLib;

namespace AncibleCoreServer.Services.Traits
{
    public class TraitService : WorldService
    {
        public override string Name => "Trait Service";

        private static TraitService _instance = null;

        private Dictionary<string, ObjectTrait> _instantTraits = new Dictionary<string, ObjectTrait>();
        private Dictionary<string, TraitData> _traits = new Dictionary<string, TraitData>();
        private Dictionary<string, SpriteTraitData> _sprites = new Dictionary<string, SpriteTraitData>();

        private string _path = string.Empty;

        public TraitService(string path)
        {
            _path = path;
        }

        public static ObjectTrait GetTrait(string name)
        {
            if (_instance._instantTraits.TryGetValue(name, out var instantTrait))
            {
                return instantTrait;
            }

            if (_instance._traits.TryGetValue(name, out var trait))
            {
                return trait.GetTraitFromData();
            }

            return null;
        }

        public static ObjectTrait GetSpriteTrait(string name)
        {
            if (_instance._sprites.TryGetValue(name, out var spriteData))
            {
                return spriteData.GetTraitFromData();
            }

            return null;
        }

        public static bool DoesTraitExist(string name)
        {
            return _instance._instantTraits.ContainsKey(name) || _instance._traits.ContainsKey(name);
        }

        public override void Start()
        {
            if (_instance == null)
            {
                _instance = this;
                var traitFiles = Directory.GetFiles(_path, $"*.{DataExtensions.TRAIT}");
                for (var i = 0; i < traitFiles.Length; i++)
                {
                    var loadResponse = FileData.LoadData<TraitData>(traitFiles[i]);
                    if (loadResponse.Success)
                    {
                        var trait = loadResponse.Data.GetTraitFromData();
                        if (trait.Instant)
                        {
                            _instantTraits.Add(loadResponse.Data.Name, trait);
                        }
                        else
                        {
                            trait.Dispose();
                            _traits.Add(loadResponse.Data.Name, loadResponse.Data);
                            if (loadResponse.Data.Type == SpriteTraitData.TYPE && loadResponse.Data is SpriteTraitData spriteTrait)
                            {
                                _sprites.Add(spriteTrait.Sprite, spriteTrait);
                            }
                        }
                    }
                }
                base.Start();
                Log($"Loaded {_instantTraits.Count} Instant traits and {_traits.Count} normal traits - Total: {_instantTraits.Count + _traits.Count}");
            }

        }

        public override void Stop()
        {
            var instants = _instantTraits.Keys.ToArray();
            for (var i = 0; i < instants.Length; i++)
            {
                _instantTraits[instants[i]].Dispose();
            }
            _instantTraits.Clear();
            _traits.Clear();
            base.Stop();
        }
    }
}