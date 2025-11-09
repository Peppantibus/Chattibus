namespace Chat.Models.Dto;

public class IncomingMessageDto
{
    // ID dell'utente destinatario (receiver)
    public Guid ToUserId { get; set; }

    // testo effettivo del messaggio
    public string Content { get; set; } = string.Empty;
}
