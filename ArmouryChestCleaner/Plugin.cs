using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ArmouryChestCleaner.Windows;
using Dalamud.Game.Inventory;
using System;
using System.Collections.Generic;
using ECommons.Logging;
using ECommons;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Game.Addon.Events;
using ArmouryChestCleaner.Commands;

namespace ArmouryChestCleaner;

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
    [PluginService] internal static IAddonEventManager AddonEventManager { get; private set; } = null!;
    [PluginService] internal static IAddonLifecycle AddonLifecycle { get; private set; } = null!;

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("ArmouryChestCleaner");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    private List<ICommand> commandsList = [];

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ECommonsMain.Init(pluginInterface, this);

        // you might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImagePath);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        commandsList = CommandHelper.BuildCommands();
        CommandHelper.RegisterCommands(CommandManager, commandsList);

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
    }

    public void Dispose()
    {
        ECommonsMain.Dispose();
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandHelper.DisposeCommands(CommandManager, commandsList);
        commandsList = [];
    }

    private unsafe void OnDropDownClick(AddonEventType type, IntPtr addon, IntPtr node)
    {
        PluginLog.Information($"OnDropDownClick call type={type} addon={addon} node={node}");
    }

    private unsafe void TooltipHandler(AddonEventType type, IntPtr addon, IntPtr node)
    {
        //PluginLog.Information("Receiving event TooltipHandler");
        var addonId = ((AtkUnitBase*)addon)->Id;

        switch (type)
        {
            case AddonEventType.MouseOver:
                AtkStage.Instance()->TooltipManager.ShowTooltip(addonId, (AtkResNode*)node, "This is a tooltip.");
                break;

            case AddonEventType.MouseOut:
                AtkStage.Instance()->TooltipManager.HideTooltip(addonId);
                break;
        }
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
