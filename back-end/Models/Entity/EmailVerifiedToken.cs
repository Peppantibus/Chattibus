namespace Chat.Models.Entity
{
    public class EmailVerifiedToken
    {
        public int Id { get; set; }
        public Guid Token { get; set; } 
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }

        public User User { get; set; } = default!;
    }
}
