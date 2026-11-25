using System;
using System.Collections.Generic;

namespace shop.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public virtual ICollection<UsersImport> UsersImports { get; set; } = new List<UsersImport>();
}
