using System.ComponentModel.DataAnnotations;

namespace CRM.ViewModels;

public class LoginVM
{
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
}