using System.ComponentModel.DataAnnotations;

namespace CRM.Models;
public class User
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string? EnName { get; set; }
    public string? ArName { get; set; }
    [Required]
    public string? Password { get; set; }
    public string? Phone { get; set; }
}
