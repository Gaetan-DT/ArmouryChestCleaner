using Dalamud.Game.Inventory;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamplePlugin;

public static class Extensions
{
    public static uint NormalizeItemId(this uint itemId)
    {
        return itemId > 1_000_000 ? itemId - 1_000_000 : itemId > 500_000 ? itemId - 500_000 : itemId;
    }

    public static InventoryType ToInventoryTypeOrThrow(this GameInventoryType gameInventoryType)
    {
        switch (gameInventoryType)
        {
            case GameInventoryType.Inventory1:
                return InventoryType.Inventory1;
            case GameInventoryType.Inventory2:
                return InventoryType.Inventory2;
            case GameInventoryType.Inventory3:
                return InventoryType.Inventory3;
            case GameInventoryType.Inventory4:
                return InventoryType.Inventory4;
            case GameInventoryType.ArmoryOffHand:
                return InventoryType.ArmoryOffHand;
            case GameInventoryType.ArmoryHead:
                return InventoryType.ArmoryHead;
            case GameInventoryType.ArmoryBody:
                return InventoryType.ArmoryBody;
            case GameInventoryType.ArmoryHands:
                return InventoryType.ArmoryHands;
            case GameInventoryType.ArmoryWaist:
                return InventoryType.ArmoryWaist;
            case GameInventoryType.ArmoryLegs:
                return InventoryType.ArmoryLegs;
            case GameInventoryType.ArmoryFeets:
                return InventoryType.ArmoryFeets;
            case GameInventoryType.ArmoryEar:
                return InventoryType.ArmoryEar;
            case GameInventoryType.ArmoryNeck:
                return InventoryType.ArmoryNeck;
            case GameInventoryType.ArmoryWrist:
                return InventoryType.ArmoryWrist;
            case GameInventoryType.ArmoryRings:
                return InventoryType.ArmoryRings;
            case GameInventoryType.ArmoryMainHand:
                return InventoryType.ArmoryMainHand;
            default:
                throw new NotImplementedException($"Not managed GameInventoryType [{gameInventoryType}]");
        }
    }

    public static GameInventoryType? ToGameInventoryTypeOrNull(this EquipSlotCategory equipSlotCategory)
    {
        if (equipSlotCategory.MainHand != 0)
        {
            return GameInventoryType.ArmoryMainHand;
        }
        else if (equipSlotCategory.OffHand != 0)
        {
            return GameInventoryType.ArmoryOffHand;
        }
        else if (equipSlotCategory.Head != 0)
        {
            return GameInventoryType.ArmoryHead;
        }
        else if (equipSlotCategory.Body != 0)
        {
            return GameInventoryType.ArmoryBody;
        }
        else if (equipSlotCategory.Gloves != 0)
        {
            return GameInventoryType.ArmoryHands;
        }
        else if (equipSlotCategory.Waist != 0)
        {
            return GameInventoryType.ArmoryWaist;
        }
        else if (equipSlotCategory.Legs != 0)
        {
            return GameInventoryType.ArmoryLegs;
        }
        else if (equipSlotCategory.Feet != 0)
        {
            return GameInventoryType.ArmoryFeets;
        }
        else if (equipSlotCategory.Ears != 0)
        {
            return GameInventoryType.ArmoryEar;
        }
        else if (equipSlotCategory.Neck != 0)
        {
            return GameInventoryType.ArmoryNeck;
        }
        else if (equipSlotCategory.Wrists != 0)
        {
            return GameInventoryType.ArmoryWrist;
        }
        else if (equipSlotCategory.FingerL != 0)
        {
            return GameInventoryType.ArmoryRings;
        }
        else if (equipSlotCategory.FingerR != 0)
        {
            return GameInventoryType.ArmoryRings;
        }
        else if (equipSlotCategory.SoulCrystal != 0)
        {
            return GameInventoryType.ArmorySoulCrystal;
        }
        else
        {
            Svc.Log.Warning($"Unable to found GameInventoryType for id {equipSlotCategory.RowId}");
            return null;
        }
    }

}
