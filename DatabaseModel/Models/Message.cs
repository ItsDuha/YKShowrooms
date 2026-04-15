using System;
using System.Collections.Generic;

namespace DatabaseModel.Models;

public partial class Message
{
    public int Id { get; set; }

    public int ConversationId { get; set; }

    public string Role { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTimeOffset? CreatedAt { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;
}
