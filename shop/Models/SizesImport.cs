using System;
using System.Collections.Generic;

namespace shop.Models;

public partial class SizesImport
{
    public int SizeId { get; set; }

    public double SizeName { get; set; }

    public virtual ICollection<OrderItemsImport> OrderItemsImports { get; set; } = new List<OrderItemsImport>();

    public virtual ICollection<StockItemsImport> StockItemsImports { get; set; } = new List<StockItemsImport>();
}
