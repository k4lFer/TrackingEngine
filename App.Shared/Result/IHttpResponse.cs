using System.Net;

namespace App.Shared.Result;

public interface IHttpResponse
{
    HttpStatusCode StatusCode { get; }
}