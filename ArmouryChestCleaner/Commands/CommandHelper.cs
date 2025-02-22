using ArmouryChestCleaner.Windows;
using Dalamud.Plugin.Services;
using System.Collections.Generic;

namespace ArmouryChestCleaner.Commands
{
    public static class CommandHelper
    {
        public static List<ICommand> BuildCommands(
            MainWindow mainWindow,
            ConfigWindow configWindow)
        {
            return
            [
                new ClearAMCommand(mainWindow, configWindow)
            ];
        }

        public static void RegisterCommands(ICommandManager commandManager, List<ICommand> commands)
        {
            foreach (var command in commands)
                commandManager.AddHandler(command.GetCommandName(), command.CreateCommandInfo());
        }

        public static void DisposeCommands(ICommandManager commandManager, List<ICommand> commands)
        {
            foreach (var command in commands)
                commandManager.RemoveHandler(command.GetCommandName());
        }
    }
}
