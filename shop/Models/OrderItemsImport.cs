using System;
using System.Collections.Generic;

namespace shop.Models;

public partial class OrderItemsImport
{
    public int OrderItemId { get; set; }

    public int OrderNumber { get; set; }

    public int ProductId { get; set; }

    public int SizeId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public virtual OrdersImport? OrdersImport { get; set; }

    public virtual ProductsImport Product { get; set; } = null!;

    public virtual SizesImport Size { get; set; } = null!;
}
