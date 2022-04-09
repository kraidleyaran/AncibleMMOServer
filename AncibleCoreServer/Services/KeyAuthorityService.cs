using System;
using AncibleCoreServer.Data;
using AncibleCoreServer.Services.Database;

namespace AncibleCoreServer.Services
{
    public class KeyAuthorityService : WorldService
    {
        public override string Name => "Key Authority Service";

        private static KeyAuthorityService _instance = null;

        public override void Start()
        {
            if (_instance == null)
            {
                _instance = this;
                base.Start();
            }
            
        }

        public static void GenerateKey(string name)
        {
            var key = Guid.NewGuid().ToString();
            var keyCollection = DatabaseService.Main.GetCollection<GeneratedKeyData>(GeneratedKeyData.TABLE);
            var existingKeyUser = keyCollection.FindOne(k => k.Name == name.ToLower());
            if (existingKeyUser == null)
            {
                var playerKey = key;
                var existingKey = keyCollection.FindOne(k => k.Key == playerKey);
                var exists = existingKey != null;
                while (exists)
                {
                    key = Guid.NewGuid().ToString();
                    var newPlayerKey = key;
                    existingKey = keyCollection.FindOne(k => k.Key == newPlayerKey);
                    exists = existingKey != null;
                }

                var generatedKey = new GeneratedKeyData { Key = key, Name = name, Claimed = false };
                keyCollection.Insert(generatedKey);
                _instance.Log($"Key Generated for {name}");
            }
            else
            {
                _instance.Log($"Key already exists for {name}");
            }
            

        }

        public static bool IsKeyValid(string key)
        {
            var keyCollection = DatabaseService.Main.GetCollection<GeneratedKeyData>(GeneratedKeyData.TABLE);
            var existingKey = keyCollection.FindOne(k => k.Key == key);
            return existingKey != null && !existingKey.Claimed;
        }

        public static bool ClaimKey(string key, string username)
        {
            var keyCollection = DatabaseService.Main.GetCollection<GeneratedKeyData>(GeneratedKeyData.TABLE);
            var existingKey = keyCollection.FindOne(k => k.Key == key);
            if (existingKey != null)
            {
                if (existingKey.Claimed)
                {
                    return false;
                }

                existingKey.Claimed = true;
                existingKey.User = username;
                keyCollection.Update(existingKey);
                return true;
            }

            return false;
        }
    }
}