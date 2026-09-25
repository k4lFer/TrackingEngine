namespace App.Shared.Common.Result;

public interface IMessageDto
{
    public IEnumerable<MessageDto?> Messages { get; set; }
}