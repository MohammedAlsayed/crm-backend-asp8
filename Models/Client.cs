using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace CRM.Models;
public class Client
{
    [Key]
    public int Id { get; set; }

    public string? ArName { get; set; }
    [Required]
    public string? EnName { get; set; }
    [Url]
    public string? Website { get; set; }
    [Phone]
    public string? Phone { get; set; }
    public string? City { get; set; }

    public ICollection<Contact>? Contacts { get; set; }

    public override string ToString() => $"Id: {Id}, Arabic Name: {ArName}, English Name: {EnName}, Website: {Website}, Phone: {Phone}, City: {City}";
}
