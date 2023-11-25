using System.ComponentModel.DataAnnotations;

namespace CRM.ViewModels;

public class RegisterVM
{
    [Required]
    public string EnName { get; set; }
    public string ArName { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
}
