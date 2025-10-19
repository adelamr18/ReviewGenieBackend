using System.ComponentModel.DataAnnotations;

namespace ReviewGenie.Domain.Entities;

public class BusinessIntegration
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid BusinessId { get; set; }
    public Business Business { get; set; } = null!;
    
    public string Platform { get; set; } = string.Empty; // "google", "yelp"
    public string ExternalAccountId { get; set; } = string.Empty;
    public string ExternalLocationId { get; set; } = string.Empty;
    
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string Scopes { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    public DateTime ConnectedAt { get; set; }
    public DateTime? LastSyncAt { get; set; }
    
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessAddress { get; set; } = string.Empty;
}
