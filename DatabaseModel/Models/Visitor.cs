using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace DatabaseModel.Models;

public partial class Visitor
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = null!;

    
    [Required(ErrorMessage = "Phone is required")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "Phone must be exactly 8 digits")]
    public string Phone { get; set; } = null!;

    [EmailAddress(ErrorMessage = "Invalid email")]
    public string? Email { get; set; }

    [RegularExpression(@"^\d{9}$", ErrorMessage = "CPR must be exactly 9 digits")]
    public string? Cpr { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a showroom")]
    public int ShowroomId { get; set; }

    public int? ProductId { get; set; }

    public int? SalespersonId { get; set; }

    public bool? IsInterested { get; set; }

    public string? DeletedFrom { get; set; } // "daily" or "missed"
    public string? FollowUpStatus { get; set; }

    public int? Score { get; set; }

    public string? Notes { get; set; }

    public bool? IsReturning { get; set; }
    public int IsDeleted { get; set; } = 0; // 0 = active, -1 = deleted
    public string? Language { get; set; }


    // Marketing source fields
    public string? MarketingSourceOther { get; set; }

    public DateTimeOffset? VisitedAt { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public string? FollowUpNotes { get; set; }

    // Navigation properties 
    public virtual Product? Product { get; set; }

    public virtual Salesperson? Salesperson { get; set; }

    [ValidateNever]
    public virtual Showroom? Showroom { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public string? MarketingSource { get; set; }


}