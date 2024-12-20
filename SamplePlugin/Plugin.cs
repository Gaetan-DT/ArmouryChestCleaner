using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using SamplePlugin.Windows;
using System.Diagnostics.CodeAnalysis;
using Dalamud.Logging.Internal;
using FFXIVClientStructs.FFXIV.Client.Game;
using Dalamud.Game.Inventory;
using Lumina.Excel.Sheets;
using System;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System.Collections;
using System.Collections.Generic;
using Lumina.Excel;
using FFXIVClientStructs.FFXIV.Common.Component.Excel;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using static FFXIVClientStructs.FFXIV.Client.Game.UI.NpcTrade;
using System.Diagnostics;

namespace SamplePlugin;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog PluginManager { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IGameInventory GameInventory { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    


    private readonly SheetsUtils sheetsUtils = new SheetsUtils(DataManager);

    private const string CommandName = "/pmycommand";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("SamplePlugin");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // you might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImagePath);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        CommandManager.AddHandler("/myTest", new CommandInfo(OnCommand)
        {
            HelpMessage = "Test"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        //ToggleMainUI();
        PluginManager.Info($"Command: {command}");
        ChatGui.Print($"Command: {command}");
        switch (command)
        {
            case "/myTest":
                RunTestCommand();
                break;
            default:
                RunDefaultCommand();
                break;
        }

        
    }

    private void RunTestCommand()
    {
        ChatGui.Print("Hello test command");
        Log.Info("Hello world");
    }

    private void RunDefaultCommand()
    {
        var armouryChestList = new List<GameInventoryType>()
        {
            //GameInventoryType.ArmoryMainHand,
            //GameInventoryType.ArmoryHead,
            //GameInventoryType.ArmoryBody,
            //GameInventoryType.ArmoryHands,
            //GameInventoryType.ArmoryLegs,
            //GameInventoryType.ArmoryFeets,
            //GameInventoryType.ArmoryEar,
            //GameInventoryType.ArmoryNeck,
            //GameInventoryType.ArmoryWrist,
            GameInventoryType.ArmoryRings,
        };
        //GameInventoryType
        foreach (var inventoryType in armouryChestList)
        {
            new ArmouryChestRemover(GameInventory, inventoryType, ChatGui, sheetsUtils)
                .Execute();
        }
    }

    private void MoveItemFromArmoryToGearSet()
    {
        //TODO
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
