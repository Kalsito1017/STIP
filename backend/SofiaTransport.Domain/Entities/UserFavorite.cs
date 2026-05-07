namespace SofiaTransport.Domain.Entities;

public class UserFavorite
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
