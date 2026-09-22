using System.Net;
using App.Shared.Result;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Helper;

public static class ResponseHelper
{
    public static IActionResult GetActionResult(IHttpResponse output)
    {
        return output.StatusCode switch
        {
            HttpStatusCode.OK => new OkObjectResult(output),
            HttpStatusCode.Created => new CreatedResult(string.Empty, output),
            HttpStatusCode.NoContent => new NoContentResult(),
            HttpStatusCode.NotFound => new NotFoundObjectResult(output),
            HttpStatusCode.BadRequest => new BadRequestObjectResult(output),
            HttpStatusCode.Conflict => new ConflictObjectResult(output),
            HttpStatusCode.UnprocessableEntity => new UnprocessableEntityObjectResult(output),
            HttpStatusCode.Forbidden => new ObjectResult(output)
            {
                StatusCode = StatusCodes.Status403Forbidden
            },
            _ => new ObjectResult(output)
            {
                StatusCode = (int)output.StatusCode
            }
        };
    }
}