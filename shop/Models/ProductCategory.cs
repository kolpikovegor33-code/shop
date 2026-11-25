using System;
using System.Collections.Generic;

namespace shop.Models;

public partial class ProductCategory
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public virtual ICollection<ProductsImport> ProductsImports { get; set; } = new List<ProductsImport>();
}
