using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Models;

public class RefreshTokens
{  
    [Key]
    public int Id { get; set; }
    public string Token { get; set; }
    public string JwtId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User User { get; set; }
    
}