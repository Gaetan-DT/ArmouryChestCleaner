using Dalamud.Game.Inventory;
using ECommons.DalamudServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamplePlugin
{
    public static class Utils
    {
        public static GameInventoryItem? GetFreeAvailableInventoryItemId()
        {
            foreach (var inventoryX in new List<GameInventoryType>() {
                GameInventoryType.Inventory1,
                GameInventoryType.Inventory2,
                GameInventoryType.Inventory3,
                GameInventoryType.Inventory4,
            })
            {
                var inventoryXItems = Svc.GameInventory.GetInventoryItems(inventoryX);
                foreach (var inventoryXItem in inventoryXItems)
                {
                    if (inventoryXItem.IsEmpty)
                        return inventoryXItem;
                }
            }
            return null;
        }
    }
}
