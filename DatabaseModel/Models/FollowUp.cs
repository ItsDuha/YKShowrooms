using System;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace DatabaseModel.Models;

public partial class FollowUp
{
    public int Id { get; set; }

    [Required]
    public int VisitorId { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public int Score { get; set; } = 0;

    public DateTimeOffset? CreatedAt { get; set; }

    [ValidateNever]
    public virtual Visitor Visitor { get; set; } = null!;
}