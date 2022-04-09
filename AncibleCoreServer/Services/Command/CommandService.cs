using System.Collections.Generic;
using AncibleCoreCommon.CommonData;
using MessageBusLib;

namespace AncibleCoreServer.Services.Command
{
    public class CommandService : WorldService
    {
        public override string Name => "Command Service";

        private static CommandService _instance = null;

        private Dictionary<string, ServerCommand> _commands = new Dictionary<string, ServerCommand>
        {
            {"createuser", new ServerCommand(CreateUser)},
            {"generatekey", new ServerCommand(GenerateKey) },
            {"userlist", new ServerCommand(UserList) },
            {"save", new ServerCommand(Save) },
            {"resetpassword", new ServerCommand(ResetPasswordForUser) },
            {"setdefaultmap", new ServerCommand(SetDefaultMap) },
            {"setstartingclasses", new ServerCommand(SetStartingClasses) },
            {"setstartingabilities", new ServerCommand(SetStartingAbilities) },
            {"setglobalcooldown", new ServerCommand(SetGlobalCooldown) },
            {"setstartingmaxinventoryslots", new ServerCommand(SetStartingMaxInventorySlots) },
            {"setdefaultinteractionrange", new ServerCommand(SetDefaultInteractionRange) },
            {"setclassexperiencesettings", new ServerCommand(SetClassExperienceSettings) },
            {"wipecharacters", new ServerCommand(WipeCharacters) },
            {"setmaxlevel", new ServerCommand(SetMaxLevel) },
            {"setdefaultcheckpoint", new ServerCommand(SetDefaultCheckpoint) },
            {"setcullingbox", new ServerCommand(SetCullingBox) },
            {"setchestticks", new ServerCommand(SetChestTicks) }
        };

        public override void Start()
        {
            if (_instance == null)
            {
                _instance = this;
                base.Start();
                SubscribeToMessages();
            }

        }

        private static void CreateUser(string[] args)
        {
            if (args.Length > 1)
            {
                _instance.SendMessage(new CreateUserMessage { Username = args[0].ToLower(), Password = args[1] });
            }
            else
            {
                _instance.Log("Requires username and password arguments");
            }
        }

        private static void GenerateKey(string[] args)
        {
            if (args.Length > 0)
            {
                for (var i = 0; i < args.Length; i++)
                {
                    KeyAuthorityService.GenerateKey(args[i]);
                }
            }
            else
            {
                _instance.Log("Requires user argument(s)");
            }
        }

        private static void UserList(string[] args)
        {
            _instance.SendMessage(UserListMessage.INSTANCE);
        }

        private static void Save(string[] args)
        {
            _instance.SendMessage(WorldSaveMessage.INSTANCE);
        }

        private static void ResetPasswordForUser(string[] args)
        {
            if (args.Length > 0)
            {
                _instance.SendMessage(new ResetPasswordForUserMessage{User = args[0].ToLower()});
            }
            else
            {
                _instance.Log("Requires user argument");
            }
        }

        private static void SetDefaultMap(string[] args)
        {
            if (args.Length > 2 && int.TryParse(args[1], out var x) && int.TryParse(args[2], out var y))
            {
                _instance.SendMessage(new SetDefaultTileMessage{Map = args[0], Position = new Vector2IntData(x,y)});
            }
            else
            {
                _instance.Log("Requires map x y arguments");
            }
        }

        private static void SetStartingClasses(string[] args)
        {
            if (args.Length > 0)
            {
                _instance.SendMessage(new SetStartingClassesMessage{Classes = args});
            }
            else
            {
                _instance.Log("Requires classes argument(s)");
            }
        }

        private static void SetStartingAbilities(string[] args)
        {
            if (args.Length > 0)
            {
                _instance.SendMessage(new SetStartingAbilitiesMessage{Abilities = args});
            }
            else
            {
                _instance.Log("Requires ability argument(s)");
            }
        }

        private static void SetGlobalCooldown(string[] args)
        {
            if (args.Length > 0 && int.TryParse(args[0], out var ticks))
            {
                _instance.SendMessage(new SetGlobalCooldownMessage{Ticks = ticks});
            }
            else
            {
                _instance.Log("Requires cooldown argument");
            }
        }

        private static void SetStartingMaxInventorySlots(string[] args)
        {
            if (args.Length > 0 && int.TryParse(args[0], out var max))
            {
                _instance.SendMessage(new SetStartingMaxInventorySlotsMessage{Max = max});
            }
            else
            {
                _instance.Log("Requires max argument");
            }
        }

        private static void SetDefaultInteractionRange(string[] args)
        {
            if (args.Length > 0 && int.TryParse(args[0], out var range))
            {
                _instance.SendMessage(new SetDefaultInteractionRangeMessage{Range = range});
            }
            else
            {
                _instance.Log($"Requires range argument");
            }
        }

        private static void SetClassExperienceSettings(string[] args)
        {
            if (args.Length > 1 && int.TryParse(args[0], out var baseExperience) && float.TryParse(args[1], out var multiplier))
            {
                _instance.SendMessage(new SetClassExperienceSettingsMessage{BaseExperience = baseExperience, ExperienceMultiplier = multiplier});
            }
            else
            {
                _instance.Log($"Requires baseExperience(int) and multiplier(float) arguments");
            }
        }

        private static void WipeCharacters(string[] args)
        {
            _instance.SendMessage(WipeCharactersMessage.INSTANCE);
        }

        private static void SetMaxLevel(string[] args)
        {
            if (int.TryParse(args[0], out var max))
            {
                _instance.SendMessage(new SetMaxLevelMessage{Max = max});
            }
            else
            {
                _instance.Log($"Requires maxLevel argument");
            }
        }

        private static void SetDefaultCheckpoint(string[] args)
        {
            if (args.Length > 0)
            {
                _instance.SendMessage(new SetDefaultCheckpointMessage{Default = args[0]});
            }
            else
            {
                _instance.Log($"Requires default argument");
            }
        }

        private static void SetCullingBox(string[] args)
        {
            if (args.Length > 1 && int.TryParse(args[0], out var x) && int.TryParse(args[1], out var y))
            {
                _instance.SendMessage(new SetCullingBoxMessage{Box = new Vector2IntData(x,y)});
            }
            else
            {
                _instance.Log($"Requires x,y arguments");
            }
        }

        private static void SetChestTicks(string[] args)
        {
            if (args.Length > 0 && int.TryParse(args[0], out var ticks))
            {
                _instance.SendMessage(new SetChestTicksMessage{Ticks = ticks});
            }
            else
            {
                _instance.Log("Requires ticks argument");
            }
        }

        private void SubscribeToMessages()
        {
            this.Subscribe<ExecuteServerCommandMessage>(ExecuteServerCommand);
        }

        private void ExecuteServerCommand(ExecuteServerCommandMessage msg)
        {
            if (_commands.TryGetValue(msg.Command, out var command))
            {
                command.Execute(msg.Arguments);
            }
            else
            {
                _instance.Log($"Invalid command: {msg.Command}");
            }
        }

    }
}