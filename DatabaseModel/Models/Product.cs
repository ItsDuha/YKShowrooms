using System;
using System.Collections.Generic;

namespace DatabaseModel.Models;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? NameAr { get; set; }

    public string? Category { get; set; }

    public string? ImageUrl { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public virtual ICollection<Visitor> Visitors { get; set; } = new List<Visitor>();
}
