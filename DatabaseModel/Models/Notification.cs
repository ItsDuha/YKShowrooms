using System;
using System.Collections.Generic;

namespace DatabaseModel.Models;

public partial class Notification
{
    public int Id { get; set; }

    public string Type { get; set; } = null!;

    public string Message { get; set; } = null!;

    public int? VisitorId { get; set; }

    public bool? IsRead { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public virtual Visitor? Visitor { get; set; }
}
