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
}
