using Dalamud.IoC;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamplePlugin;

public class SheetsUtils
{
    public readonly ExcelSheet<Lumina.Excel.Sheets.Item> sheetItem;

    public SheetsUtils(IDataManager DataManager)
    {
        sheetItem = DataManager.GetExcelSheet<Lumina.Excel.Sheets.Item>();
    }

    public Item? GetItemInfoFromSheets(uint itemId)
    {
        var normalizeItemId = itemId.NormalizeItemId();
        if (normalizeItemId <= 0)
            return null;
        if (!sheetItem.TryGetRow(normalizeItemId, out var itemInfo))
            return null;
        return itemInfo;
    }

}
