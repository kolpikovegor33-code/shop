using System;
using System.Collections.Generic;

namespace shop.Models;

public partial class Manufacturer
{
    public int ManufacturerId { get; set; }

    public string Manufacturer1 { get; set; } = null!;

    public virtual ICollection<ProductsImport> ProductsImports { get; set; } = new List<ProductsImport>();
}
