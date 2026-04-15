using System;
using System.ComponentModel.DataAnnotations;
namespace DatabaseModel.Models;

public partial class Admin
{
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}