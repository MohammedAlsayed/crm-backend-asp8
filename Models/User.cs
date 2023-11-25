using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CRM.Models;
public class User: IdentityUser
{
    [Required]
    public string? EnName { get; set; }
    public string? ArName { get; set; }
}
