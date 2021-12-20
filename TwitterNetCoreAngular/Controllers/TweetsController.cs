using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TwitterNetCoreAngular.Controllers;

[ApiController]
[Route("[controller]")]

public class TweetsController : ControllerBase
{
    private readonly ServiceHub _serviceHub;

    public TweetsController(ServiceHub serviceHub)
    {
        _serviceHub = serviceHub;
    }

    // GET: <TweetsController>

    [HttpGet]
    public IActionResult GetTweetsAsync()
    {
        return Ok(_serviceHub.getAll());
    }

}
