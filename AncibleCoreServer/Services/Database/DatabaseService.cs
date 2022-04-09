using System.Collections.Generic;
using System.IO;
using System.Linq;
using AncibleCoreServer.Data;
using FileDataLib;
using LiteDB;
using MessageBusLib;
using Newtonsoft.Json;

namespace AncibleCoreServer.Services.Database
{
    public class DatabaseService : WorldService
    {
        public override string Name => "Database Service";
        public static LiteDatabase Main => _instance._mainDatabase;

        private static DatabaseService _instance = null;

        private LiteDatabase _mainDatabase = null;
        private DatabaseSettings _settings = null;
        private List<string> _characterNames = new List<string>();
        private string _settingsPath = string.Empty;

        public DatabaseService(string settingsPath)
        {
            _settingsPath = settingsPath;
        }

        public static string CreateUserDatabase(string username)
        {
            var path = $@"{_instance._settings.UserFolder}\{username}.db";
            var database = new LiteDatabase(path);
            database.Dispose();
            return path;
        }

        public static string CreateCharacterDatabase(string characterName)
        {
            var path = $@"{_instance._settings.CharacterFolder}\{characterName}.db";
            var database = new LiteDatabase(path);

            database.Dispose();
            return path;
        }

        public static bool CharacterNameExists(string name)
        {
            return _instance._characterNames.Contains(name.ToLower());
        }

        public static void AddCharacterName(string name)
        {
            _instance._characterNames.Add(name);
            _instance._mainDatabase.GetCollection<WorldCharacterName>(WorldCharacterName.TABLE).Insert(new WorldCharacterName{Name = name});
        }

        public static LiteDatabase OpenDatabase(string path)
        {
            return new LiteDatabase($@"{path}");
        }

        public override void Start()
        {
            if (_instance == null && File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                var databasesettings = JsonConvert.DeserializeObject<DatabaseSettings>(json);
                if (databasesettings != null)
                {
                    _settings = databasesettings;
                    _instance = this;
                    _mainDatabase = new LiteDatabase(_settings.Main);
                    _characterNames = _mainDatabase.GetCollection<WorldCharacterName>(WorldCharacterName.TABLE).FindAll().Select(n => n.Name).ToList();
                    var userCollection = _mainDatabase.GetCollection<WorldUser>(WorldUser.TABLE);
                    var activeUsers = userCollection.FindAll().Where(u => u.Active).ToArray();
                    for (var i = 0; i < activeUsers.Length; i++)
                    {
                        activeUsers[i].Active = false;
                        userCollection.Update(activeUsers[i]);
                    }
                    SubscribeToMessages();
                    base.Start();
                }
                else
                {
                    Log("Invalid Database Settings");
                }

            }
            else
            {
                Log($"Cannot find path {_settingsPath}");
            }

        }

        private void SubscribeToMessages()
        {
            this.Subscribe<WipeCharactersMessage>(WipeCharacters);
        }

        private void WipeCharacters(WipeCharactersMessage msg)
        {
            var users = Main.GetCollection<WorldUser>(WorldUser.TABLE).FindAll().ToArray();
            var characterCount = 0;
            for (var u = 0; u < users.Length; u++)
            {
                var user = users[u];
                var userLogFile = user.DataPath;
                if (!string.IsNullOrEmpty(userLogFile))
                {
                    var extensionIndex = userLogFile.IndexOf(".");
                    if (extensionIndex >= 0)
                    {
                        userLogFile = userLogFile.Insert(userLogFile.IndexOf("."), $"-log");
                        if (File.Exists(userLogFile))
                        {
                            FileData.DeleteFile(userLogFile);
                        }
                    }
                }
                var userDatabase = OpenDatabase(user.DataPath);


                userDatabase.DropCollection(CharacterCombatStats.TABLE);
                userDatabase.DropCollection(CharacterGrowthStats.TABLE);
                var characterCollection = userDatabase.GetCollection<WorldCharacter>(WorldCharacter.TABLE);
                var characters = characterCollection.FindAll().ToArray();
                for (var i = 0; i < characters.Length; i++)
                {
                    var logFile = characters[i].Path;
                    if (!string.IsNullOrEmpty(logFile))
                    {
                        var extensionIndex = logFile.IndexOf(".");
                        if (extensionIndex >= 0)
                        {
                            logFile = logFile.Insert(logFile.IndexOf("."), $"-log");
                            if (File.Exists(logFile))
                            {
                                FileData.DeleteFile(logFile);
                            }
                        }
                    }
                    var response = FileData.DeleteFile(characters[i].Path);
                    var filePassed = false;
                    if (response.Success)
                    {
                        filePassed = true;
                    }
                    else
                    {
                        Log(response.HasException ? $"Exception while deleting {characters[i].Path} - {response.Exception}" : $"Unknown error while deleting {characters[i].Path}");
                    }

                    var databasePassed = characterCollection.Delete(characters[i]._id);
                    if (!databasePassed)
                    {
                        Log($"Could not delete {characters[i].Name} from database");
                    }
                    else if (filePassed)
                    {
                        characterCount++;
                    }
                }

                userDatabase.DropCollection(WorldCharacter.TABLE);
                userDatabase.Dispose();

            }

            Main.DropCollection(WorldCharacterName.TABLE);

            var mainLogFile = _settings.Main;
            if (!string.IsNullOrEmpty(mainLogFile))
            {
                var extensionIndex = mainLogFile.IndexOf(".");
                if (extensionIndex >= 0)
                {
                    mainLogFile = mainLogFile.Insert(mainLogFile.IndexOf("."), $"-log");
                    if (File.Exists(mainLogFile))
                    {
                        FileData.DeleteFile(mainLogFile);
                    }
                }
            }

            Log($"Wiped {characterCount} Characters from {users.Length} users");

        }

        public override void Stop()
        {
            base.Stop();
            _mainDatabase.Dispose();
            _mainDatabase = null;
        }
    }
}