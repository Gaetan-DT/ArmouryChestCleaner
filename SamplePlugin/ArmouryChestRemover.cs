using Dalamud.Game.Inventory;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamplePlugin
{
    internal class ArmouryChestRemover
    {
        private readonly IGameInventory gameInventory;
        private readonly GameInventoryType gameInventoryType;
        private readonly IChatGui? chatGui;
        private readonly SheetsUtils sheetsUtils;

        public ArmouryChestRemover(
            IGameInventory GameInventory,
            GameInventoryType GameInventoryType,
            IChatGui? ChatGui,
            SheetsUtils sheetsUtils)
        {

            gameInventory = GameInventory;
            gameInventoryType = GameInventoryType;
            chatGui = ChatGui;
            this.sheetsUtils = sheetsUtils;
        }

        public void Execute()
        {
            var armoryItemList = GetArmoryItem();
            var gearSetItemList = GetGearSetItemList();
            chatGui?.Print($"For Armoury:[{gameInventoryType}], " +
                $"Item found in armoury:[{armoryItemList.Count}], " +
                $"Item found in gear set:[{gearSetItemList.Count}]");
            chatGui?.Print($"Armoury item Ids:[{string.Join(", ", armoryItemList)}]");
            chatGui?.Print($"Gear set item Ids:[{string.Join(", ", gearSetItemList)}]");
            var amouryItemInGearSetIdList = GetItemIdNotInGearSet(armoryItemList, gearSetItemList);
            chatGui?.Print($"For Armoury:[{gameInventoryType}]: Found [{amouryItemInGearSetIdList.Count}] item(s) in Gear Set");
            /*
            chatGui?.Print($"Found list: [{string.Join(", ", amouryItemInGearSetIdList)}]");
            foreach (var item in amouryItemInGearSetIdList)
            {
                var itemName = GetItemInfoFromSheets(item)?.Name;
                if (itemName != null)
                    chatGui?.Print($"Item name: [{itemName}]");
            }
            */
        }

        private List<uint> GetItemIdNotInGearSet(List<uint> armouryIds, List<uint> gearSetIds)
        {
            List<uint> result = [];
            foreach (var armouryId in armouryIds)
            {
                if (gearSetIds.Contains(armouryId))
                    result.Add(armouryId);
            }
            return result;
        }

        private List<uint> GetArmoryItem()
        {
            List<uint> listOfItemId = [];
            var mainHandInventory = gameInventory.GetInventoryItems(gameInventoryType);
            var filedItem = 0;
            foreach (var item in mainHandInventory)
            {
                if (!item.IsEmpty)
                {
                    listOfItemId.Add(item.ItemId);
                    filedItem++;
                }
            }
            return listOfItemId;
        }

        private List<uint> GetGearSetItemList()
        {
            List<uint> listItemIdForInventoryType = [];
            unsafe
            {
                var gearSetModule = RaptureGearsetModule.Instance();
                foreach (var gearSetEntry in gearSetModule->Entries)
                {
                    foreach (var entryItem in gearSetEntry.Items)    
                    {
                        if (entryItem.ItemId == 0)
                            continue;
                        var sheetsItem = GetItemInfoFromSheets(entryItem.ItemId);
                        if (sheetsItem == null)
                        {
                            chatGui?.Print($"Unable to find sheetsItem for id: {entryItem.ItemId}");
                            continue;
                        }
                            
                        var equipSlotCategory = sheetsItem?.EquipSlotCategory.Value;
                        var gameInventoryType = ToGameInventoryType(sheetsItem?.EquipSlotCategory.Value);
                        //chatGui?.Print($"[{gearSetEntry.NameString}] [{entryItem.ItemId}] [{sheetsItem?.Name}] [{equipSlotCategory?.RowId}] [{gameInventoryType}]");
                        if (gameInventoryType == this.gameInventoryType)
                        {
                            listItemIdForInventoryType.Add(entryItem.ItemId.NormalizeItemId());
                            if (gameInventoryType != GameInventoryType.ArmoryRings)
                                break;
                        }
                    }
                }
            }
            return listItemIdForInventoryType;
        }

        private void GetGearSetItemList(List<uint> itemToLookup)
        {
            Dictionary<uint, bool> mapItemToLookUpIsFound = new Dictionary<uint, bool>();
            foreach (var item in itemToLookup)
                mapItemToLookUpIsFound.Add(item, false);
            var foundCount = 0;
            unsafe
            {
                var gearSetModule = RaptureGearsetModule.Instance();
                var gearSetEntry = gearSetModule->Entries;
                foreach (var entry in gearSetEntry)
                {
                    foreach (var item in entry.Items)
                    {
                        
                        var itemId = item.ItemId.NormalizeItemId();
                        if (itemId == 0u)
                            continue;
                        var found = itemToLookup.Contains(itemId);
                        if (found)
                        {
                            foundCount++;
                            mapItemToLookUpIsFound.Remove(itemId);
                            mapItemToLookUpIsFound.Add(itemId, true);
                        }
                    }
                }
            }
            /*chatGui?.Print($"For [{gameInventoryType}]: [{foundCount}/{itemToLookup.Count}] [{itemToLookup.Count-foundCount}] Can be removed");
            chatGui?.Print($"Not found item list:");
            foreach (var item in mapItemToLookUpIsFound)
            {
                if (!item.Value)
                    DisplayItemInfo(item.Key);
            }*/
        }

        private void DisplayItemInfo(uint id)
        {
            var sheetItem = sheetsUtils.sheetItem;
            if (!sheetItem.TryGetRow(id, out var row))
            {
                chatGui?.Print($"Name: Not found Id:[{id}]");
                return;
            }
            chatGui?.Print($"Name: {row.Name} Id:[{id}]");
        }

        private GameInventoryType? ToGameInventoryType(EquipSlotCategory? equipSlotCategory)
        {
            if (equipSlotCategory?.MainHand != 0)
            {
                //chatGui?.Print($"equipSlotCategory?.MainHand={equipSlotCategory?.MainHand}");
                return GameInventoryType.ArmoryMainHand;
            }
            else if (equipSlotCategory?.OffHand != 0)
            {
                return GameInventoryType.ArmoryOffHand;
            }
            else if (equipSlotCategory?.Head != 0)
            {
                return GameInventoryType.ArmoryHead;
            }
            else if (equipSlotCategory?.Body != 0)
            {
                return GameInventoryType.ArmoryBody;
            }
            else if (equipSlotCategory?.Gloves != 0)
            {
                return GameInventoryType.ArmoryHands;
            }
            else if (equipSlotCategory?.Waist != 0)
            {
                return GameInventoryType.ArmoryWaist;
            }
            else if (equipSlotCategory?.Legs != 0)
            {
                return GameInventoryType.ArmoryLegs;
            }
            else if (equipSlotCategory?.Feet != 0)
            {
                return GameInventoryType.ArmoryFeets;
            }
            else if (equipSlotCategory?.Ears != 0)
            {
                return GameInventoryType.ArmoryEar;
            }
            else if (equipSlotCategory?.Neck != 0)
            {
                return GameInventoryType.ArmoryNeck;
            }
            else if (equipSlotCategory?.Wrists != 0)
            {
                return GameInventoryType.ArmoryWrist;
            }
            else if (equipSlotCategory?.FingerL != 0)
            {
                return GameInventoryType.ArmoryRings;
            }
            else if (equipSlotCategory?.FingerR != 0)
            {
                return GameInventoryType.ArmoryRings;
            }
            else if (equipSlotCategory?.SoulCrystal != 0)
            {
                return GameInventoryType.ArmorySoulCrystal;
            }
            else
            {
                chatGui?.Print($"Unable to found GameInventoryType for id {equipSlotCategory?.RowId}");
                return null;
            }
        }

        private Item? GetItemInfoFromSheets(uint itemId)
        {
            var normalizeItemId = itemId.NormalizeItemId();
            if (normalizeItemId <= 0)
                return null;
            if (!sheetsUtils.sheetItem.TryGetRow(normalizeItemId, out var itemInfo))
                return null;
            return itemInfo;
        }
    }
}
