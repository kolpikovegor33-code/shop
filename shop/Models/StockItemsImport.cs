using System;
using System.Collections.Generic;

namespace shop.Models;

public partial class StockItemsImport
{
    public int ProductId { get; set; }

    public int SizeId { get; set; }

    public int QuantityForOrders { get; set; }

    public virtual SizesImport Size { get; set; } = null!;
}
