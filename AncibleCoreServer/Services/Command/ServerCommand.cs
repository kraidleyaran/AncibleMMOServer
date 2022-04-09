using System;

namespace AncibleCoreServer.Services.Command
{
    public class ServerCommand
    {
        public Action<string[]> Action;

        public ServerCommand(Action<string[]> action)
        {
            Action = action;
        }

        public void Execute(string[] args)
        {
            Action(args);
        }
    }
}