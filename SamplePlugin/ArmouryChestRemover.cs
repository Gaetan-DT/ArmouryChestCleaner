using Dalamud.Game.Inventory;
using ECommons.DalamudServices;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;

namespace ArmouryChestCleaner
{
    internal class ArmouryChestRemover
    {
        private readonly GameInventoryType gameInventoryTypeToClear;
        private readonly SheetsUtils sheetsUtils;

        public ArmouryChestRemover(GameInventoryType GameInventoryType,SheetsUtils sheetsUtils)
        {
            gameInventoryTypeToClear = GameInventoryType;
            this.sheetsUtils = sheetsUtils;
        }

        public bool Execute()
        {
            List<GameInventoryItem> armoryItemList = GetArmoryItem();
            List<uint> gearSetItemList = GetGearSetItemList();
            Log.LogInfo($"For Armoury:[{gameInventoryTypeToClear}], " +
                $"Item found in armoury:[{armoryItemList.Count}], " +
                $"Item found in gear set:[{gearSetItemList.Count}]");
            List<GameInventoryItem> amouryItemInGearSetIdList = GetItemIdNotInGearSet(armoryItemList, gearSetItemList);
            Log.LogInfo($"For Armoury:[{gameInventoryTypeToClear}]: Found [{amouryItemInGearSetIdList.Count}] item(s) in Gear Set");
            PrintListItemIdAsItem(amouryItemInGearSetIdList);

            if (amouryItemInGearSetIdList != null && amouryItemInGearSetIdList?.Count > 0)
            {
                foreach (var amouryItemInGearSetId in amouryItemInGearSetIdList)
                {
                    var isItemMoved = MoveItemToInventory(amouryItemInGearSetId);
                    if (!isItemMoved)
                    {
                        Log.LogInfo($"Unable to move item [{amouryItemInGearSetId.ItemId}] to inventory", true);
                        return false;
                    }
                    EzThrottler.Throttle("WaitForItemToMove");
                }
                Log.LogInfo($"Moved [{amouryItemInGearSetIdList?.Count}] item(s) to inventory", true);
            } 
            else
            {
                Log.LogInfo($"No item to move for [{gameInventoryTypeToClear}]", true);
            }
            return true;
        }

        private List<GameInventoryItem> GetItemIdNotInGearSet(
            List<GameInventoryItem> gameInventoryItemList, 
            List<uint> gearSetIds)
        {
            List<GameInventoryItem> result = [];
            foreach (var gameInventoryItem in gameInventoryItemList)
            {
                if (!gearSetIds.Contains(gameInventoryItem.ItemId))
                    result.Add(gameInventoryItem);
            }
            return result;
        }

        private List<GameInventoryItem> GetArmoryItem()
        {
            List<GameInventoryItem> listOfItemId = [];
            var gameInventoryItemsListForType = Svc.GameInventory.GetInventoryItems(gameInventoryTypeToClear);
            foreach (var gameInventoryItem in gameInventoryItemsListForType)
            {
                if (!gameInventoryItem.IsEmpty)
                {
                    listOfItemId.Add(gameInventoryItem);
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
                            Log.LogInfo($"Unable to find sheetsItem for id: {entryItem.ItemId}", true);
                            continue;
                        } 
                            
                        var equipSlotCategory = sheetsItem?.EquipSlotCategory.Value;
                        var gameInventoryType = sheetsItem?.EquipSlotCategory.Value.ToGameInventoryTypeOrNull();
                        //chatGui?.Print($"[{gearSetEntry.NameString}] [{entryItem.ItemId}] [{sheetsItem?.Name}] [{equipSlotCategory?.RowId}] [{gameInventoryType}]");
                        if (gameInventoryType == this.gameInventoryTypeToClear)
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

        private unsafe bool MoveItemToInventory(GameInventoryItem gameInventoryItem)
        {
            var itemNameStr = GetItemInfoFromSheets(gameInventoryItem.ItemId)
                ?.Name.ToString() 
                ?? gameInventoryItem.ItemId.ToString();
            Log.LogDebug($"Move itemId {itemNameStr}");
            GameInventoryItem? freeGameInventoryItem = Utils.GetFreeAvailableInventoryItemId();
            if (freeGameInventoryItem == null)
            {
                Log.LogDebug($"No free inventory slot");
                return false;
            }
            uint freeGameInventoryItemSlotId = ((GameInventoryItem)freeGameInventoryItem).InventorySlot;
            var freeGameInventoryItemType = ((GameInventoryItem)freeGameInventoryItem).ContainerType.ToInventoryTypeOrThrow();
            InventoryManager.Instance()->MoveItemSlot(
                srcContainer: gameInventoryTypeToClear.ToInventoryTypeOrThrow(),
                srcSlot: (ushort)gameInventoryItem.InventorySlot,
                dstContainer: freeGameInventoryItemType,
                dstSlot: (ushort)freeGameInventoryItemSlotId,
                unk: 1 // ???
                );
            Log.LogDebug($"Moving itemId: [{gameInventoryItem.InventorySlot}] to inventory Id: [{freeGameInventoryItemSlotId}]");
            Log.LogDebug($"Item moved !");
            return true;
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

        private void PrintListItemIdAsItem(List<GameInventoryItem> gameInventoryItemList)
        {
            foreach (var gameInventoryItem in gameInventoryItemList) {
                var itemInfo = GetItemInfoFromSheets(gameInventoryItem.ItemId)?.Name.ToString() ?? "Unknown item";
                Log.LogDebug($"Item: [{itemInfo}]");
            }
        }
    }
}
