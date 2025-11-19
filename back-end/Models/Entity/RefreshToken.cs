namespace Chat.Models.Entity;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string? ReplacedByToken { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    //utile per sapere da che IP vengono generati e revocati i token, utile per rilevare movimenti sospetti
    public string? CreatedByIp { get; set; }
    public string? RevokedByIp { get; set; }

    // Utente proprietario
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
}
