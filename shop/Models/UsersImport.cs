using System;
using System.Collections.Generic;

namespace shop.Models;

public partial class UsersImport
{
    public int UserId { get; set; }

    public string LastName { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Surname { get; set; }

    public string Login { get; set; } = null!;

    public int RoleId { get; set; }

    public virtual ICollection<OrdersImport> OrdersImports { get; set; } = new List<OrdersImport>();

    public virtual Role Role { get; set; } = null!;
}
