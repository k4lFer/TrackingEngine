namespace App.Shared.Result;

public interface IMessageDto
{
    public IEnumerable<MessageDto?> Messages { get; set; }
}