using System;
using System.Collections.Generic;

namespace shop.Models;

public partial class ProductsImport
{
    public int ProductId { get; set; }

    public int CategoryId { get; set; }

    public int SubcategoryId { get; set; }

    public string Picture { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public int ManufacturerId { get; set; }

    public string Description { get; set; } = null!;

    public string Structure { get; set; } = null!;

    public decimal Price { get; set; }

    public virtual ProductCategory Category { get; set; } = null!;

    public virtual Manufacturer Manufacturer { get; set; } = null!;

    public virtual ICollection<OrderItemsImport> OrderItemsImports { get; set; } = new List<OrderItemsImport>();

    public virtual ProductsSubcategory Subcategory { get; set; } = null!;
}
