using System;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace DatabaseModel.Models;

public partial class MarketingSource
{
    public int Id { get; set; }

    [Required]
    public int VisitorId { get; set; }

    public string? Source { get; set; }

    public string? SourceOther { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    [ValidateNever]
    public virtual Visitor Visitor { get; set; } = null!;
}

public static class MarketingSourceOptions
{
    public const string Instagram = "Instagram";
    public const string TikTok = "TikTok";
    public const string Website = "Website";
    public const string Friend = "Friend";
    public const string WalkIn = "Walk-in";
    public const string YouTube = "YouTube";

    public static readonly string[] All =
    [
        Instagram, TikTok, Website, Friend, WalkIn, YouTube
    ];
}