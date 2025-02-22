using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using SamplePlugin.Windows;
using Dalamud.Game.Inventory;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using ECommons.Logging;
using ECommons;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Addon.Events;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonRelicNoteBook;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using ECommons.Automation.UIInput;

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
    [PluginService] internal static IAddonEventManager AddonEventManager { get; private set; } = null!;
    [PluginService] internal static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
    
    

    private readonly SheetsUtils sheetsUtils = new SheetsUtils(DataManager);

    private const string CommandName = "/clearam";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("SamplePlugin");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

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

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "MonsterNote", OnMonsterNotePostSetupTrigger);
    }

    public void Dispose()
    {
        ECommonsMain.Dispose();
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
        switch (command)
        {
            case CommandName:
                if (ExtractArmouryToClearFromArgs(args, out List<GameInventoryType> inventoryTypeToClear))
                {
                    PluginManager.Info($"Clearing armoury chest: {string.Join(", ", inventoryTypeToClear)}");
                    ChatGui.Print($"Clearing armoury chest: {string.Join(", ", inventoryTypeToClear)}");
                    RunClearArmouryChestCommand(inventoryTypeToClear);
                } else
                {
                    ChatGui.Print("Invalid arguments. Usage: /clearam <all|mainhand|head|body|hands|legs|feets|ear|neck|wrist|rings>");
                }
                    break;
            default:
                ChatGui.Print($"Unknown command: {command} : {args}");
                break;
        }
    }

    private bool ExtractArmouryToClearFromArgs(string args, out List<GameInventoryType> inventoryTypeToClear)
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

    private unsafe void OnMonsterNotePostSetupTrigger(AddonEvent type, AddonArgs args)
    {
        PluginLog.Information("OnMonsterNotePostSetupTrigger");
        AtkUnitBase* addon = (AtkUnitBase*)args.Addon;
        AtkResNode* targetNode = addon->GetNodeById(22);
        AtkResNode* dropDownList = addon->GetNodeById(38);
        //dropDownList->ChildNode[1].ToggleVisibility(false);

        var childNode = &dropDownList->ChildNode[1];
        var foo = dropDownList->GetAsAtkComponentDropdownList()->UldManager.NodeList[1];

        targetNode->NodeFlags |= NodeFlags.EmitsEvents | NodeFlags.RespondToMouse | NodeFlags.HasCollision;
        foo->NodeFlags |= NodeFlags.EmitsEvents | NodeFlags.RespondToMouse | NodeFlags.HasCollision;

        AddonEventManager.AddEvent((nint)addon, (nint)foo, AddonEventType.MouseOver, OnDropDownClick);

        AddonEventManager.AddEvent((nint)addon, (nint)targetNode, AddonEventType.MouseOver, TooltipHandler);
        AddonEventManager.AddEvent((nint)addon, (nint)targetNode, AddonEventType.MouseOut, TooltipHandler);

        //var addon2 = (AtkUnitBase*)Svc.GameGui.GetAddonByName("MonsterNote");

        PluginLog.Information("Sending event");
        AtkEventType atkEventType = AtkEventType.MouseOver;

        var evt = targetNode->AtkEventManager.Event;

        var eventParam = (int)evt->Param;
        var atkEvent = targetNode->AtkEventManager.Event;

        addon->ReceiveEvent(atkEventType, eventParam, atkEvent);
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

    private unsafe void RunTestCommand()
    {
        /*
        var addon = Svc.GameGui.GetAddonByName("MonsterNote");
        if (addon == nint.Zero)
        {
            PluginLog.Information("Unable to find addon");
            return;
        }
        AtkUnitBase* atkBase = (AtkUnitBase*)addon; // Addon
        AtkResNode* node = atkBase->GetNodeById(38);
        var foo = node->GetAsAtkComponentDropdownList()->UldManager.NodeList[1];
        //node->ToggleVisibility(false);
        PluginLog.Information("Sending event");
        var btnRes = foo->GetComponent()->OwnerNode->AtkResNode;
        var evt = (AtkEvent*)btnRes.AtkEventManager.Event;
        atkBase->ReceiveEvent(AtkEventType.MouseClick, (int)evt->Param, btnRes.AtkEventManager.Event);
        */
    }

    private unsafe void RunTestCommandYesNo()
    {
        var addon = Svc.GameGui.GetAddonByName("SelectYesno");
        if (addon == nint.Zero)
        {
            PluginLog.Information("Unable to find addon");
            return;
        }
        AddonSelectYesno* addonSelectYesno = (AddonSelectYesno*)addon;
        AtkUnitBase * atkBase = (AtkUnitBase*)addon; // Addon
        AtkComponentButton* yesButton = addonSelectYesno->YesButton; // Target

        var btnRes = yesButton->AtkComponentBase.OwnerNode->AtkResNode;
        var evt = (AtkEvent*)btnRes.AtkEventManager.Event;
        atkBase->ReceiveEvent(evt->State.EventType, (int)evt->Param, btnRes.AtkEventManager.Event);
    }

    private unsafe void RunTestCommandInventory()
    {
        var addon = Svc.GameGui.GetAddonByName("InventoryGrid3E");
        if (addon == nint.Zero)
        {
            PluginLog.Information("Unable to find addon");
        }
        else
        {
            PluginLog.Information("AddonInventoryGrid found");
            AddonInventoryGrid* inventoryGrid3E = (AddonInventoryGrid*)addon;
            AtkComponentDragDrop* srcSlot = inventoryGrid3E->Slots[0];
            AtkComponentDragDrop* destSlot = inventoryGrid3E->Slots[11];
            PluginLog.Information($"srcSlot -> OwnerNode NodeId:[{srcSlot->OwnerNode->NodeId}] Type:[{srcSlot->OwnerNode->Type}]");
            PluginLog.Information($"srcSlot -> {srcSlot->GetIconId()}");
            PluginLog.Information($"destSlot -> OwnerNode NodeId:[{destSlot->OwnerNode->NodeId}] Type:[{destSlot->OwnerNode->Type}]");
            PluginLog.Information($"destSlot -> {destSlot->GetIconId()}");
        }
    }

    private unsafe void RunTestCancelLogoutCommand()
    {
        ChatGui.Print("Hello test command");
        PluginLog.Information("Hello world");
        bool contains = false;
        string stringParam = Svc.Data.GetExcelSheet<Addon>()?.GetRow(115).Text.ExtractText()!;
        PluginLog.Information($"stringParam = {stringParam}");

        AtkUnitBase* addon = null;

        for (var i = 1; i < 100; i++)
        {
            try
            {
                addon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("SelectYesno", i);
                if (addon == null) break;//return null;
                if(IsAddonReady(addon))
                {
                    var textNode = addon->UldManager.NodeList[15]->GetAsAtkTextNode();
                    var text = GenericHelpers.ReadSeString(&textNode->NodeText).ExtractText().Replace(" ", "");
                    PluginLog.Information($"text = {text}");

                    if(contains ?
                        text.ContainsAny(stringParam.Replace(" ", ""))
                        : text.EqualsAny(stringParam.Replace(" ", ""))
                        )
                    {
                        PluginLog.Information($"SelectYesno {stringParam.Print()} addon {i}");
                        break;//return addon;
                    }
                }
            }
            catch(Exception e)
            {
                e.Log();
                break; // return null;
            }
        }
        if (addon != null)
            new AddonMaster.SelectYesno((nint)addon).No();
        //return null;
    }

    public unsafe static bool IsAddonReady(AtkUnitBase* Addon)
    {
        return Addon->IsVisible && Addon->UldManager.LoadedState == AtkLoadState.Loaded && Addon->IsFullyLoaded();
    }

    private void  RunClearArmouryChestCommand(List<GameInventoryType> inventoryTypeToClear)
    {
        foreach (var inventoryType in inventoryTypeToClear)
        {
            new ArmouryChestRemover(GameInventoryType: inventoryType,sheetsUtils: sheetsUtils).Execute();
        }
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
