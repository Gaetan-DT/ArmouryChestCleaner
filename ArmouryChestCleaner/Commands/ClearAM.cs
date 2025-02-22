using ArmouryChestCleaner.UseCases;
using Dalamud.Game.Command;
using Dalamud.Game.Inventory;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmouryChestCleaner.Commands
{
    public class ClearAM: ICommand
    {
        public const string CommandName = "/clearam";
        public const string CommandUsage = $"Usage: {CommandName} <all|mainhand|head|body|hands|legs|feets|ear|neck|wrist|rings>";



        public string GetCommandName()
        {
            return CommandName;
        }

        public CommandInfo CreateCommandInfo()
        {
            return new CommandInfo(OnCommand)
            {
                HelpMessage = CommandUsage
            };
        }

        public void OnCommand(string command, string args)
        {
            Svc.Log.Info($"Command: {command}");
            switch (command)
            {
                case CommandName:
                    if (ExtractArmouryToClearFromArgs(args, out var inventoryTypeToClear))
                    {
                        Svc.Log.Info($"Clearing armoury chest: {string.Join(", ", inventoryTypeToClear)}");
                        Svc.Chat.Print($"Clearing armoury chest: {string.Join(", ", inventoryTypeToClear)}");
                        RunClearArmouryChestCommand(inventoryTypeToClear);
                    }
                    else
                    {
                        Svc.Chat.Print($"Invalid arguments. {CommandUsage}");
                    }
                    break;
                default:
                    Svc.Chat.Print($"Unknown command: {command} : {args}");
                    break;
            }
        }

        private static bool ExtractArmouryToClearFromArgs(string args, out List<GameInventoryType> inventoryTypeToClear)
        {
            inventoryTypeToClear = new List<GameInventoryType>();
            var mapArgInventoryType = new Dictionary<string, GameInventoryType>()
        {
            { "mainhand", GameInventoryType.ArmoryMainHand },
            { "head", GameInventoryType.ArmoryHead },
            { "body", GameInventoryType.ArmoryBody },
            { "hands", GameInventoryType.ArmoryHands },
            { "legs", GameInventoryType.ArmoryLegs },
            { "feets", GameInventoryType.ArmoryFeets },
            { "ear", GameInventoryType.ArmoryEar },
            { "neck", GameInventoryType.ArmoryNeck },
            { "wrist", GameInventoryType.ArmoryWrist },
            { "rings", GameInventoryType.ArmoryRings }
        };

            if (args.Contains("all"))
            {
                inventoryTypeToClear.Add(GameInventoryType.ArmoryMainHand);
                inventoryTypeToClear.Add(GameInventoryType.ArmoryHead);
                inventoryTypeToClear.Add(GameInventoryType.ArmoryBody);
                inventoryTypeToClear.Add(GameInventoryType.ArmoryHands);
                inventoryTypeToClear.Add(GameInventoryType.ArmoryLegs);
                inventoryTypeToClear.Add(GameInventoryType.ArmoryFeets);
                inventoryTypeToClear.Add(GameInventoryType.ArmoryEar);
                inventoryTypeToClear.Add(GameInventoryType.ArmoryNeck);
                inventoryTypeToClear.Add(GameInventoryType.ArmoryWrist);
                inventoryTypeToClear.Add(GameInventoryType.ArmoryRings);
                return true;
            }

            foreach (var inventoryType in mapArgInventoryType)
            {
                if (args.Contains(inventoryType.Key))
                {
                    inventoryTypeToClear.Add(inventoryType.Value);
                }
            }

            return inventoryTypeToClear.Count > 0;
        }

        private void RunClearArmouryChestCommand(List<GameInventoryType> inventoryTypeToClear)
        {
            var sheetsUtils = SheetsUtils.GetOrCreate();
            foreach (var inventoryType in inventoryTypeToClear)
            {
                new ArmouryChestRemoverUseCase(GameInventoryType: inventoryType, sheetsUtils: sheetsUtils).Execute();
            }
        }
    }
}
