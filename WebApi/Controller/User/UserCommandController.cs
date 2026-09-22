using Cortex.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controller.User;

[ApiController]
[Route("api/user")]
[Tags("Users")]
[Produces("application/json")]
public class UserCommandController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserCommandController(IMediator mediator)
    {
        _mediator = mediator;
    }
}