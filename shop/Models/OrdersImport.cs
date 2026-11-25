using System;
using System.Collections.Generic;

namespace shop.Models;

public partial class OrdersImport
{
    public int OrderNumber { get; set; }

    public DateOnly OrderDate { get; set; }

    public int UserId { get; set; }

    public virtual OrderItemsImport OrderNumberNavigation { get; set; } = null!;

    public virtual UsersImport User { get; set; } = null!;
}
