using System;
using System.Collections.Generic;

namespace DatabaseModel.Models;

public partial class Feedback
{
    public int Id { get; set; }

    public int? VisitorId { get; set; }

    public int ShowroomId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public virtual Showroom Showroom { get; set; } = null!;

    public virtual Visitor? Visitor { get; set; }
}
