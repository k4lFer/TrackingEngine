using System.Net;
using System.Text.Json.Serialization;

namespace App.Shared.Result;

public class OutputPort<T> : IHttpResponse, IMessageDto
{
    private OutputPort(HttpStatusCode statusCode, T? data, IEnumerable<MessageDto> messages, bool isSuccess)
    {
        StatusCode = statusCode;
        Data = data;
        Messages = messages;
        IsSuccess = isSuccess;
    }
    private bool IsSuccess { get; set; }
    
    [JsonIgnore]
    public HttpStatusCode StatusCode { get; }
    public T? Data { get; }
    public IEnumerable<MessageDto?> Messages { get; set; }
    
    public static OutputPort<T> Success(T? data, HttpStatusCode statusCode = HttpStatusCode.OK, string message = "Operation completed successfully.")
    {
        return new OutputPort<T>(statusCode, data, [new MessageDto(code: statusCode.ToString().ToUpper(), message: message)], true);
    }

    public static OutputPort<T> Failure(
        HttpStatusCode statusCode,
        params MessageDto[] messages)
    {
        return new OutputPort<T>(statusCode, default, messages, false);
    }
}