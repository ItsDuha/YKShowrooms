using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DatabaseModel.Models;

public partial class Showroom
{
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = null!;

    public string? NameAr { get; set; }

    public string? Location { get; set; }

    [Phone]
    public string? Phone { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Salesperson> Salespeople { get; set; } = new List<Salesperson>();

    public virtual ICollection<Visitor> Visitors { get; set; } = new List<Visitor>();
}