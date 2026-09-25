using System.Net;

namespace App.Shared.Common.Result;

public interface IHttpResponse
{
    HttpStatusCode StatusCode { get; }
}