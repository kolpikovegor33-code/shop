using System;
using System.Collections.Generic;

namespace shop.Models;

public partial class ProductsSubcategory
{
    public int SubcategoryId { get; set; }

    public string SubcategoryName { get; set; } = null!;

    public virtual ICollection<ProductsImport> ProductsImports { get; set; } = new List<ProductsImport>();
}
