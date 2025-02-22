using ECommons.DalamudServices;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace ArmouryChestCleaner;

public class SheetsUtils
{
    private static SheetsUtils? SSelf = null;

    public static SheetsUtils GetOrCreate()
    {
        if (SSelf == null)
            SSelf = new SheetsUtils();
        return SSelf;
    }

    public readonly ExcelSheet<Item> sheetItem;

    private SheetsUtils()
    {
        sheetItem = Svc.Data.GetExcelSheet<Item>();
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
