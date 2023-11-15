using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CRM.Models;
public class Contact
{

    [Key]
    public int Id { get; set; }
    [Required]
    public string? EnName { get; set; }
    public string? ArName { get; set; }
    public string? Grade { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public string? Department { get; set; }
    [Phone]
    public string? Phone { get; set; }
    
    [ForeignKey(nameof(Client))]
    public int ClientId { get; set; }
    
    // override to string method
    public override string ToString() => $"EnName: {EnName}, ArName: {ArName}, Grade: {Grade}, Email: {Email}, Department: {Department}, Phone: {Phone}";
}