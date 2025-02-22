using Dalamud.Plugin.Services;
using System.Collections.Generic;

namespace ArmouryChestCleaner.Commands
{
    public static class CommandHelper
    {
        public static List<ICommand> BuildCommands()
        {
            return
            [
                new ClearAM()
            ];
        }

        public static void RegisterCommands(ICommandManager commandManager)
        {
            foreach (var command in BuildCommands())
                commandManager.AddHandler(command.GetCommandName(), command.CreateCommandInfo());
        }
    }
}
