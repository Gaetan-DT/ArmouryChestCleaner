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
    public class ClearAMCommand: ICommand
    {
        private const string CommandName = "/clearam";
        private const string CommandUsage = $"Usage: {CommandName} <{CommandArgAll}|{CommandArgMainHand}|{CommandArgHead}|{CommandArgBody}|" +
            $"{CommandArgHands}|{CommandArgLegs}|{CommandArgFeets}|{CommandArgEar}|{CommandArgNeck}|{CommandArgWrist}|{CommandArgRings}>";
        private const string CommandArgAll = "all";
        private const string CommandArgMainHand = "mainhand";
        private const string CommandArgHead = "head";
        private const string CommandArgBody = "body";
        private const string CommandArgHands = "hands";
        private const string CommandArgLegs = "legs";
        private const string CommandArgFeets = "feets";
        private const string CommandArgEar = "ear";
        private const string CommandArgNeck = "neck";
        private const string CommandArgWrist = "wrist";
        private const string CommandArgRings = "rings";


        private readonly ArmouryChestRemoverUseCase armouryChestRemoverUseCase = new();

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
            { CommandArgMainHand, GameInventoryType.ArmoryMainHand },
            { CommandArgHead, GameInventoryType.ArmoryHead },
            { CommandArgBody, GameInventoryType.ArmoryBody },
            { CommandArgHands, GameInventoryType.ArmoryHands },
            { CommandArgLegs, GameInventoryType.ArmoryLegs },
            { CommandArgFeets, GameInventoryType.ArmoryFeets },
            { CommandArgEar, GameInventoryType.ArmoryEar },
            { CommandArgNeck, GameInventoryType.ArmoryNeck },
            { CommandArgWrist, GameInventoryType.ArmoryWrist },
            { CommandArgRings, GameInventoryType.ArmoryRings }
        };

            if (args.Contains(CommandArgAll))
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
            foreach (var inventoryType in inventoryTypeToClear)
                armouryChestRemoverUseCase.Execute(inventoryType);
        }
    }
}
