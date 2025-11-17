using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace Chat.Models.Entity;

//metodo per definire i campi univoci sul database
[Index(nameof(Email), IsUnique = true)]
[Index(nameof(Username), IsUnique = true)]
public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    [Required]
    [MaxLength(64)]
    public string Email { get; set; } = string.Empty;
    [Required]
    [MaxLength(64)]
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public bool EmailVerified { get; set; } = false;
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public DateTime? PasswordUpdatedAt { get; set; }


    // Navigation properties
    public ICollection<Message> SentMessages { get; set; } = new List<Message>(); //messaggi inviati
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>(); //messaggi ricevuti
    public ICollection<Friend> Friends { get; set; } = new List<Friend>();    // Amici che ho aggiunto io
    public ICollection<Friend> FriendOf { get; set; } = new List<Friend>();  // Utenti che mi hanno aggiunto
    public ICollection<FriendRequest> SentRequests { get; set; } = new List<FriendRequest>(); //richieste inviate
    public ICollection<FriendRequest> ReceivedRequests { get; set; } = new List<FriendRequest>(); //richieste ricevute
    public ICollection<EmailVerifiedToken> EmailVerifiedTokens { get; set; } = new List<EmailVerifiedToken>(); //token verifica email
    public ICollection<PasswordResetToken> PasswordResetTokens  { get; set; } = new List<PasswordResetToken>(); //token reset password

}
