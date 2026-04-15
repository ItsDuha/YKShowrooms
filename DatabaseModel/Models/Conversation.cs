using System;
using System.Collections.Generic;

namespace DatabaseModel.Models;

public partial class Conversation
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public DateTimeOffset? CreatedAt { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
