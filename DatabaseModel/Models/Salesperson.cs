using System;
using System.Collections.Generic;

namespace DatabaseModel.Models;

public partial class Salesperson
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? ShowroomId { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public virtual Showroom? Showroom { get; set; }

    public virtual ICollection<Visitor> Visitors { get; set; } = new List<Visitor>();
}
